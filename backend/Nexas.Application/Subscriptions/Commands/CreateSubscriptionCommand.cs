using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;
using Nexas.Domain.Enums;

namespace Nexas.Application.Subscriptions.Commands;

/// <summary>
/// Comando para criar uma nova assinatura recorrente.
/// </summary>
public record CreateSubscriptionCommand(string PlanName, decimal Amount) : IRequest<int>;

public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, int>
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

    public async Task<int> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        // 1. Busca Usuário logado
        var user = await _userContext.GetCurrentUserAsync();

        var dbUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken)
            ?? throw new Exception("Usuário não encontrado.");

        // 2. VERIFICAÇÃO/CRIAÇÃO DO CLIENTE NO ASAAS
        if (string.IsNullOrEmpty(dbUser.AsaasCustomerId))
        {
            var customerId = await _asaasService.CreateCustomerAsync(dbUser, cancellationToken);
            dbUser.UpdateAsaasCustomerId(customerId);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // 3. Criar a assinatura no banco (inicia inativa/pendente)
        var subscription = Subscription.Create(
            dbUser.Id, 
            request.PlanName, 
            DateTime.UtcNow, 
            null, 
            false, 
            SubscriptionStatus.Pending);

        subscription.SetUser(dbUser);
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        // 4. Gerar a assinatura recorrente no Asaas
        var asaasSubId = await _asaasService.CreateSubscriptionAsync(subscription, request.Amount, cancellationToken);

        // 5. Atualizar assinatura com ID do Asaas e ativar
        subscription.UpdateAsaasSubscriptionId(asaasSubId);
        subscription.Activate();
        await _context.SaveChangesAsync(cancellationToken);

        // 6. Registrar o pagamento da mensalidade correspondente no banco
        var payment = SubscriptionPayment.Create(
            subscription.Id,
            request.Amount,
            DateTime.UtcNow,
            SubscriptionPaymentStatus.Pending,
            null);

        _context.SubscriptionPayments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return subscription.Id;
    }
}
