using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;
using Nexas.Domain.Enums;

namespace Nexas.Application.Purchases.Commands;

// DTO para dados do cartão vindo do front
public record CreditCardInfo(
    string HolderName, 
    string Number, 
    string ExpiryMonth, 
    string ExpiryYear, 
    string Ccv, 
    string HolderCpfCnpj);

// DTO de retorno com dados para o front (ex: PIX)
public record PurchaseResponseDto(
    int PurchaseId,
    string Status,
    string? PixQrCode = null,
    string? PixCopyPaste = null,
    string AsaasPaymentId = "");

public record CreatePurchaseCommand(
    int CourseId, 
    decimal Amount, 
    string PaymentMethod, 
    string? Cpf = null,
    CreditCardInfo? Card = null) : IRequest<PurchaseResponseDto>;

public class CreatePurchaseCommandHandler : IRequestHandler<CreatePurchaseCommand, PurchaseResponseDto>
{
    private readonly INexasDbContext _context;
    private readonly IAsaasService _asaasService;
    private readonly IUserContextService _userContext;

    public CreatePurchaseCommandHandler(
        INexasDbContext context, 
        IAsaasService asaasService, 
        IUserContextService userContext)
    {
        _context = context;
        _asaasService = asaasService;
        _userContext = userContext;
    }

    public async Task<PurchaseResponseDto> Handle(CreatePurchaseCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _userContext.GetCurrentUserAsync();
        var userId = currentUser.Id;

        // 1. Busca Usuário e Curso (Garante dados para o Asaas como FullName e CpfCnpj)
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new Exception("Usuário não encontrado.");

        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken)
            ?? throw new Exception("Curso não encontrado.");

        // VERIFICAÇÕES DE REGRAS DE NEGÓCIO (FLUXO 4 e 5)
        var isAlreadyPaid = await _context.Purchases
            .AnyAsync(p => p.UserId == user.Id && p.CourseId == request.CourseId && p.Status == PurchaseStatus.Approved, cancellationToken);
        if (isAlreadyPaid)
            throw new Exception("Você já possui acesso a este curso.");

        var pendingPurchase = await _context.Purchases
            .Where(p => p.UserId == user.Id && p.CourseId == request.CourseId && p.Status == PurchaseStatus.Pending)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (pendingPurchase != null)
        {
            if (pendingPurchase.PaymentMethod == "PIX" && !string.IsNullOrEmpty(pendingPurchase.AsaasPaymentId))
            {
                // Tenta reaproveitar o PIX
                try
                {
                    var qrCodeData = await _asaasService.GetPixQrCodeAsync(pendingPurchase.AsaasPaymentId, cancellationToken);
                    return new PurchaseResponseDto(pendingPurchase.Id, "PENDING", qrCodeData.EncodedImage, qrCodeData.Payload, pendingPurchase.AsaasPaymentId);
                }
                catch (Exception ex)
                {
                    // Failed to retrieve QR code from Asaas.
                    // DO NOT cancel the purchase. Just return without QR code.
                    return new PurchaseResponseDto(pendingPurchase.Id, "PENDING", null, null, pendingPurchase.AsaasPaymentId);
                }
            }
            else
            {
                // Se for cartão ou falhou, lança erro para usuário tratar na tela
                throw new Exception("Você já possui uma transação em andamento para este curso.");
            }
        }

        if (string.IsNullOrEmpty(user.AsaasCustomerId))
        {
            if (request.Card != null || !string.IsNullOrWhiteSpace(request.Cpf))
            {
                var profileName = string.IsNullOrWhiteSpace(user.FullName)
                    ? (request.Card?.HolderName ?? user.ExternalId)
                    : user.FullName!;

                var profileCpfCnpj = string.IsNullOrWhiteSpace(user.CpfCnpj)
                    ? (request.Cpf ?? request.Card?.HolderCpfCnpj)
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
        else if (!string.IsNullOrWhiteSpace(request.Cpf) && string.IsNullOrWhiteSpace(user.CpfCnpj))
        {
            // O cliente jÃ¡ existe no Asaas, mas nÃ£o tinha CPF e agora foi informado
            user.UpdateProfile(user.FullName ?? user.ExternalId, request.Cpf);
            await _asaasService.UpdateCustomerAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 3. Criar a entidade de compra
        var purchase = Purchase.Create(user.Id, course.Id, request.Amount, request.PaymentMethod);
        
        // Associa instâncias para que o serviço tenha acesso aos dados de navegação
        purchase.SetUser(user);
        purchase.SetCourse(course);

        _context.Purchases.Add(purchase);
        await _context.SaveChangesAsync(cancellationToken);

        // 4. Gerar pagamento no Gateway (passando dados do cartão se houver)
        var result = await _asaasService.CreatePaymentAsync(purchase, request.Card, cancellationToken);

        // 5. Atualiza o ID externo e persiste
        // O result.AsaasPaymentId deve ser retornado pelo serviço no DTO de resposta
        purchase.UpdateAsaasPaymentId(result.AsaasPaymentId);
        await _context.SaveChangesAsync(cancellationToken);

        return result;
    }
}