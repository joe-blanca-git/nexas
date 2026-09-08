using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Enums;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Nexas.Application.Webhooks.Commands;

public class ProcessAsaasWebhookCommand : IRequest<bool>
{
    public string Event { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
    public string? ExternalReference { get; set; }
}

public class ProcessAsaasWebhookCommandHandler : IRequestHandler<ProcessAsaasWebhookCommand, bool>
{
    private readonly INexasDbContext _context;
    private readonly IPaymentEventPublisher _paymentEventPublisher;
    private readonly Microsoft.Extensions.Logging.ILogger<ProcessAsaasWebhookCommandHandler> _logger;

    public ProcessAsaasWebhookCommandHandler(
        INexasDbContext context,
        IPaymentEventPublisher paymentEventPublisher,
        Microsoft.Extensions.Logging.ILogger<ProcessAsaasWebhookCommandHandler> logger)
    {
        _context = context;
        _paymentEventPublisher = paymentEventPublisher;
        _logger = logger;
    }

    public async Task<bool> Handle(ProcessAsaasWebhookCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Processando webhook para PaymentId: {request.PaymentId}, Event: {request.Event}, CustomerId: {request.CustomerId}, ExtRef: {request.ExternalReference}");

        Nexas.Domain.Entities.Purchase? purchase = null;

        // 1. Tentar encontrar por AsaasPaymentId exato
        if (!string.IsNullOrEmpty(request.PaymentId))
        {
            purchase = await _context.Purchases
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.AsaasPaymentId == request.PaymentId, cancellationToken);
        }

        // 2. Tentar encontrar por ExternalReference (que é o ID da Purchase no nosso banco)
        if (purchase == null && !string.IsNullOrEmpty(request.ExternalReference) && int.TryParse(request.ExternalReference, out int purchaseId))
        {
            purchase = await _context.Purchases
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == purchaseId, cancellationToken);

            if (purchase != null)
            {
                _logger.LogInformation($"Compra encontrada por ExternalReference: Id={purchase.Id}. Atualizando AsaasPaymentId para: {request.PaymentId}");
                purchase.UpdateAsaasPaymentId(request.PaymentId);
            }
        }

        // 3. Tentar encontrar por CustomerId (Asaas Customer) e vincular a uma compra do usuário
        if (purchase == null && !string.IsNullOrEmpty(request.CustomerId))
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.AsaasCustomerId == request.CustomerId, cancellationToken);

            if (user != null)
            {
                // Busca a compra mais recente do usuário (independente do status atual)
                purchase = await _context.Purchases
                    .Include(p => p.User)
                    .Where(p => p.UserId == user.Id)
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (purchase != null)
                {
                    _logger.LogInformation($"[WEBHOOK ASAAS] Compra recente Id={purchase.Id} encontrada para usuário {user.Id} ({user.ExternalId}) por CustomerId: {request.CustomerId}. Vinculando AsaasPaymentId: {request.PaymentId}");
                    purchase.UpdateAsaasPaymentId(request.PaymentId);
                }
                else if (request.Event == "PAYMENT_RECEIVED" || request.Event == "PAYMENT_CONFIRMED")
                {
                    // Caso o usuário pagou mas não havia uma Purchase criada no banco
                    var firstCourse = await _context.Courses.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync(cancellationToken);
                    if (firstCourse != null)
                    {
                        _logger.LogInformation($"[WEBHOOK ASAAS] Criando nova compra e matrícula automática para usuário {user.Id} no curso {firstCourse.Id}.");
                        purchase = Nexas.Domain.Entities.Purchase.Create(user.Id, firstCourse.Id, 0, "PIX");
                        purchase.SetUser(user);
                        purchase.SetCourse(firstCourse);
                        purchase.UpdateAsaasPaymentId(request.PaymentId);
                        _context.Purchases.Add(purchase);
                    }
                }
            }
        }

        if (purchase != null)
        {
            _logger.LogInformation($"[WEBHOOK ASAAS] Compra Id={purchase.Id}, Status atual={purchase.Status}. Processando evento {request.Event}...");
            return await ProcessPurchaseAsync(purchase, request.Event, cancellationToken);
        }

        _logger.LogWarning($"[WEBHOOK ASAAS] Nenhuma compra encontrada ou criada para o PaymentId Asaas: {request.PaymentId}, CustomerId: {request.CustomerId}, ExtRef: {request.ExternalReference}");
        return false;
    }

    private async Task<bool> ProcessPurchaseAsync(Nexas.Domain.Entities.Purchase purchase, string eventType, CancellationToken cancellationToken)
    {
        var externalUserId = purchase.User.ExternalId;
        
        if (eventType == "PAYMENT_RECEIVED" || eventType == "PAYMENT_CONFIRMED")
        {
            if (purchase.Status == PurchaseStatus.Approved) 
            {
                _logger.LogInformation($"Compra Id={purchase.Id} já estava aprovada. Ignorando.");
                return true; // Idempotência
            }

            purchase.Approve();
            _logger.LogInformation($"Compra Id={purchase.Id} aprovada com sucesso. Criando matrícula...");

            // Garantir que a matrícula (Enrollment) seja criada
            bool enrollmentExists = await _context.Enrollments.AnyAsync(e => e.UserId == purchase.UserId && e.CourseId == purchase.CourseId, cancellationToken);
            if (!enrollmentExists)
            {
                var enrollment = Nexas.Domain.Entities.Enrollment.Create(purchase.UserId, purchase.CourseId, EnrollmentOrigin.Purchase);
                _context.Enrollments.Add(enrollment);
                _logger.LogInformation($"Matrícula criada para o usuário {purchase.UserId} no curso {purchase.CourseId}.");
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Alterações salvas no banco de dados para a compra Id={purchase.Id}.");

            if (externalUserId != null)
            {
                _logger.LogInformation($"Enviando evento de SignalR para o usuário {externalUserId}...");
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
            if (externalUserId != null)
            {
                await _paymentEventPublisher.PublishPaymentCanceledAsync(externalUserId, "AVULSO", purchase.CourseId);
            }
            return true;
        }
        else if (eventType == "PAYMENT_OVERDUE")
        {
            if (purchase.Status == PurchaseStatus.Expired) return true;
            purchase.Expire();
            await _context.SaveChangesAsync(cancellationToken);
            if (externalUserId != null)
            {
                await _paymentEventPublisher.PublishPaymentCanceledAsync(externalUserId, "AVULSO", purchase.CourseId);
            }
            return true;
        }

        return false;
    }
}
