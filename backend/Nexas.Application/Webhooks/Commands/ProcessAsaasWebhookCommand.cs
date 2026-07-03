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
}
