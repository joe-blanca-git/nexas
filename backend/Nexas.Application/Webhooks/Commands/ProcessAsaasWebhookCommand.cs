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
            if (purchase.Status == PurchaseStatus.Approved) return true; // Idempotência

            purchase.Approve();

            // Garantir que a matrícula (Enrollment) seja criada
            bool enrollmentExists = await _context.Enrollments.AnyAsync(e => e.UserId == purchase.UserId && e.CourseId == purchase.CourseId, cancellationToken);
            if (!enrollmentExists)
            {
                var enrollment = Nexas.Domain.Entities.Enrollment.Create(purchase.UserId, purchase.CourseId, EnrollmentOrigin.Purchase);
                _context.Enrollments.Add(enrollment);
            }

            await _context.SaveChangesAsync(cancellationToken);

            if (externalUserId != null)
            {
                await _paymentEventPublisher.PublishPaymentConfirmedAsync(externalUserId, "AVULSO", purchase.CourseId);
            }
            return true;
        }
        else if (eventType == "PAYMENT_REFUNDED")
        {
            if (purchase.Status == PurchaseStatus.Refunded) return true;

            purchase.Refund();

            // Desativar matrícula
            var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.UserId == purchase.UserId && e.CourseId == purchase.CourseId, cancellationToken);
            if (enrollment != null)
            {
                enrollment.Deactivate();
            }

            await _context.SaveChangesAsync(cancellationToken);

            if (externalUserId != null)
            {
                await _paymentEventPublisher.PublishPaymentRefundedAsync(externalUserId, "AVULSO", purchase.CourseId);
            }
            return true;
        }
        else if (eventType == "PAYMENT_DELETED" || eventType == "PAYMENT_CANCELED")
        {
            if (purchase.Status == PurchaseStatus.Canceled) return true;
            purchase.Cancel();
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        else if (eventType == "PAYMENT_OVERDUE")
        {
            if (purchase.Status == PurchaseStatus.Expired) return true;
            purchase.Expire();
            await _context.SaveChangesAsync(cancellationToken);
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
            if (payment.Status == SubscriptionPaymentStatus.Paid) return true; // Idempotência

            payment.UpdateStatus(SubscriptionPaymentStatus.Paid);
            subscription.Activate();
            await _context.SaveChangesAsync(cancellationToken);

            if (externalUserId != null)
            {
                await _paymentEventPublisher.PublishPaymentConfirmedAsync(externalUserId, "ANUAL", 0);
            }
            return true;
        }
        else if (eventType == "PAYMENT_REFUNDED")
        {
            if (payment.Status == SubscriptionPaymentStatus.Refunded) return true;

            payment.UpdateStatus(SubscriptionPaymentStatus.Refunded);
            subscription.Deactivate(SubscriptionStatus.Canceled);
            
            await _context.SaveChangesAsync(cancellationToken);

            if (externalUserId != null)
            {
                await _paymentEventPublisher.PublishPaymentRefundedAsync(externalUserId, "ANUAL", 0);
            }
            return true;
        }
        else if (eventType == "PAYMENT_DELETED" || eventType == "PAYMENT_CANCELED")
        {
            if (payment.Status == SubscriptionPaymentStatus.Canceled) return true;
            payment.Cancel();
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        else if (eventType == "PAYMENT_OVERDUE")
        {
            if (payment.Status == SubscriptionPaymentStatus.Expired) return true;
            payment.Expire();
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        return false;
    }
}
