using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.Purchases.Commands; 
using Nexas.Domain.Entities;
using Nexas.Domain.Enums;

namespace Nexas.Application.Subscriptions.Commands;

public record SubscriptionResponseDto(
    int SubscriptionId, 
    string Status, 
    string? AsaasSubscriptionId = null,
    string? PixQrCode = null,
    string? PixCopyPaste = null,
    string AsaasPaymentId = "");

public record CreateSubscriptionCommand(
    string PlanName, 
    decimal Amount, 
    string PaymentMethod, 
    CreditCardInfo? Card = null) : IRequest<SubscriptionResponseDto>;

public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, SubscriptionResponseDto>
{
    private readonly INexasDbContext _context;
    private readonly IAsaasService _asaasService;
    private readonly IUserContextService _userContext;

    public CreateSubscriptionCommandHandler(
        INexasDbContext context, 
        IAsaasService asaasService, 
        IUserContextService userContext)
    {
        _context = context;
        _asaasService = asaasService;
        _userContext = userContext;
    }

    public async Task<SubscriptionResponseDto> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _userContext.GetCurrentUserAsync();
        
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == currentUser.Id, cancellationToken)
            ?? throw new Exception("Usuário não encontrado.");

        // VERIFICAÇÕES DE REGRAS DE NEGÓCIO (FLUXO 4 e 5)
        var jaPossui = await _context.Subscriptions
            .AnyAsync(s => s.UserId == user.Id && s.Status == SubscriptionStatus.Active, cancellationToken);
        
        if (jaPossui)
            throw new Exception("Você já possui uma assinatura ativa.");

        var pendente = await _context.SubscriptionPayments
            .Include(sp => sp.Subscription)
            .Where(sp => sp.Subscription.UserId == user.Id && sp.Status == SubscriptionPaymentStatus.Pending)
            .OrderByDescending(sp => sp.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (pendente != null)
        {
            if (!string.IsNullOrEmpty(pendente.AsaasPaymentId))
            {
                // Tenta reaproveitar a pendência (Assinatura PIX/Cartão que está aguardando algo)
                try
                {
                    // Como não tem método de pagamento na entidade no momento, assumimos PIX para tentar pegar QR
                    var qrCodeData = await _asaasService.GetPixQrCodeAsync(pendente.AsaasPaymentId, cancellationToken);
                    return new SubscriptionResponseDto(pendente.SubscriptionId, "PENDING", pendente.AsaasPaymentId);
                }
                catch
                {
                    pendente.Cancel();
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
            else
            {
                throw new Exception("Você já possui uma transação em andamento para sua assinatura.");
            }
        }

        if (string.IsNullOrEmpty(user.AsaasCustomerId))
        {
            if (request.Card != null)
            {
                var profileName = string.IsNullOrWhiteSpace(user.FullName)
                    ? request.Card.HolderName
                    : user.FullName!;

                var profileCpfCnpj = string.IsNullOrWhiteSpace(user.CpfCnpj)
                    ? request.Card.HolderCpfCnpj
                    : user.CpfCnpj!;

                if (!string.IsNullOrWhiteSpace(profileName) && !string.IsNullOrWhiteSpace(profileCpfCnpj))
                {
                    user.UpdateProfile(profileName, profileCpfCnpj);
                }
            }

            var customerId = await _asaasService.CreateCustomerAsync(user, cancellationToken);
            user.UpdateAsaasCustomerId(customerId);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 1. CORREÇÃO DO ERRO CS7036: Fornecendo os 7 argumentos exigidos pela Factory do Domínio
        // A ordem deve ser: userId, planName, startDate, endDate, active, status, asaasSubscriptionId
        var subscription = Subscription.Create(
            user.Id,
            request.PlanName,
            DateTime.UtcNow,
            null,
            true,
            SubscriptionStatus.Pending,
            null);

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        var result = await _asaasService.CreateSubscriptionAsync(subscription, request.Amount, request.Card, cancellationToken, 7);

        // CORREÇÃO DO AVISO CS8604: Garantindo que o ID não seja nulo (usa ?? string.Empty)
        subscription.UpdateAsaasSubscriptionId(result.AsaasSubscriptionId ?? string.Empty);
        
        if (!string.IsNullOrEmpty(result.AsaasPaymentId))
        {
            var subPayment = SubscriptionPayment.Create(
                subscription.Id, 
                request.Amount,
                DateTime.UtcNow,
                Nexas.Domain.Enums.SubscriptionPaymentStatus.Pending,
                result.AsaasPaymentId);
            
            _context.SubscriptionPayments.Add(subPayment);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return result;
    }
}