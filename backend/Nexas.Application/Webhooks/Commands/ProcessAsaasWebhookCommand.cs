using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Enums;
using System.Text.Json;

namespace Nexas.Application.Webhooks.Commands;

public class ProcessAsaasWebhookCommand : IRequest<bool>
{
    public string Event { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
}

public class ProcessAsaasWebhookCommandHandler : IRequestHandler<ProcessAsaasWebhookCommand, bool>
{
    private readonly INexasDbContext _context;
    private readonly IPaymentEventPublisher _paymentEventPublisher;

    public ProcessAsaasWebhookCommandHandler(
        INexasDbContext context,
        IPaymentEventPublisher paymentEventPublisher)
    {
        _context = context;
        _paymentEventPublisher = paymentEventPublisher;
    }

    public async Task<bool> Handle(ProcessAsaasWebhookCommand request, CancellationToken cancellationToken)
    {
        // 1. Tentar encontrar como Purchase (Avulso)
        var purchase = await _context.Purchases
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.AsaasPaymentId == request.PaymentId, cancellationToken);

        if (purchase != null)
        {
            return await ProcessPurchaseAsync(purchase, request.Event, cancellationToken);
        }

        // 2. Tentar encontrar como SubscriptionPayment (Assinatura)
        var subscriptionPayment = await _context.SubscriptionPayments
            .Include(p => p.Subscription)
            .Include(p => p.Subscription.User)
            .FirstOrDefaultAsync(p => p.AsaasPaymentId == request.PaymentId, cancellationToken);

        if (subscriptionPayment != null)
        {
            return await ProcessSubscriptionAsync(subscriptionPayment, request.Event, cancellationToken);
        }

        // Pagamento não encontrado em nossa base
        return false;
    }

    private async Task<bool> ProcessPurchaseAsync(Nexas.Domain.Entities.Purchase purchase, string eventType, CancellationToken cancellationToken)
    {
        var externalUserId = purchase.User.ExternalId;
        
        if (eventType == "PAYMENT_RECEIVED" || eventType == "PAYMENT_CONFIRMED")
        {
            purchase.Approve();
            await _context.SaveChangesAsync(cancellationToken);

            if (externalUserId != null)
            {
                await _paymentEventPublisher.PublishPaymentConfirmedAsync(externalUserId, "AVULSO", purchase.CourseId);
            }
            return true;
        }
        else if (eventType == "PAYMENT_REFUNDED" || eventType == "PAYMENT_DELETED")
        {
            purchase.Refund();
            await _context.SaveChangesAsync(cancellationToken);

            if (externalUserId != null)
            {
                await _paymentEventPublisher.PublishPaymentRefundedAsync(externalUserId, "AVULSO", purchase.CourseId);
            }
            return true;
        }

        return false;
    }

    private async Task<bool> ProcessSubscriptionAsync(Nexas.Domain.Entities.SubscriptionPayment payment, string eventType, CancellationToken cancellationToken)
    {
        var externalUserId = payment.Subscription.User.ExternalId;
        var subscription = payment.Subscription;

        if (eventType == "PAYMENT_RECEIVED" || eventType == "PAYMENT_CONFIRMED")
        {
            payment.UpdateStatus(SubscriptionPaymentStatus.Paid);
            subscription.Activate();
            await _context.SaveChangesAsync(cancellationToken);

            if (externalUserId != null)
            {
                await _paymentEventPublisher.PublishPaymentConfirmedAsync(externalUserId, "ANUAL", 0);
            }
            return true;
        }
        else if (eventType == "PAYMENT_REFUNDED" || eventType == "PAYMENT_DELETED")
        {
            payment.UpdateStatus(SubscriptionPaymentStatus.Failed);
            // IMPORTANTE: Aqui revogamos APENAS a assinatura. O acesso avulso a outros cursos permanece inalterado.
            subscription.Deactivate(SubscriptionStatus.Canceled);
            
            await _context.SaveChangesAsync(cancellationToken);

            if (externalUserId != null)
            {
                await _paymentEventPublisher.PublishPaymentRefundedAsync(externalUserId, "ANUAL", 0);
            }
            return true;
        }

        return false;
    }
}
