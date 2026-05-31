using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

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

        // 2. VERIFICAÇÃO/CRIAÇÃO DO CLIENTE NO ASAAS
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