using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Enums;

namespace Nexas.Application.Checkout.Queries;

public class GetCheckoutPendenciasQueryHandler : IRequestHandler<GetCheckoutPendenciasQuery, CheckoutPendenciasResponseDto>
{
    private readonly INexasDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAsaasService _asaasService;

    public GetCheckoutPendenciasQueryHandler(
        INexasDbContext context,
        ICurrentUserService currentUserService,
        IAsaasService asaasService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _asaasService = asaasService;
    }

    public async Task<CheckoutPendenciasResponseDto> Handle(GetCheckoutPendenciasQuery request, CancellationToken cancellationToken)
    {
        var externalId = _currentUserService.ExternalId;
        if (externalId == null)
            return new CheckoutPendenciasResponseDto { TemPendencia = false };

        var user = await _context.Users.FirstOrDefaultAsync(u => u.ExternalId == externalId, cancellationToken);
        if (user == null)
            return new CheckoutPendenciasResponseDto { TemPendencia = false };

        var response = new CheckoutPendenciasResponseDto { TemPendencia = false, JaPago = false };

        if (request.TipoCompra == "AVULSO" && request.CursoId.HasValue)
        {
            // Check if ALREADY PAID
            var isAlreadyPaid = await _context.Purchases
                .AnyAsync(p => p.UserId == user.Id && p.CourseId == request.CursoId.Value && p.Status == PurchaseStatus.Approved, cancellationToken);
            
            if (isAlreadyPaid)
            {
                response.JaPago = true;
                return response;
            }

            var pendingPurchase = await _context.Purchases
                .Where(p => p.UserId == user.Id && p.CourseId == request.CursoId.Value && p.Status == PurchaseStatus.Pending)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (pendingPurchase != null)
            {
                response.TemPendencia = true;
                response.Status = "PENDING";
                response.MetodoPagamento = pendingPurchase.PaymentMethod;

                if (pendingPurchase.PaymentMethod == "PIX" && !string.IsNullOrEmpty(pendingPurchase.AsaasPaymentId))
                {
                    try
                    {
                        var qrCodeData = await _asaasService.GetPixQrCodeAsync(pendingPurchase.AsaasPaymentId, cancellationToken);
                        response.PixCopiaECola = qrCodeData.Payload;
                        response.QrCodeBase64 = qrCodeData.EncodedImage;
                        response.Mensagem = "Você já possui um PIX aguardando pagamento para este item.";
                    }
                    catch (Exception ex)
                    {
                        // Failed to retrieve QR Code from Asaas (might be expired, deleted, or API error)
                        // We DO NOT cancel the purchase here automatically anymore. 
                        // The user will just see that it is pending but without a QR Code.
                        response.Mensagem = $"Houve um erro ao recuperar o QR Code do PIX. (Erro: {ex.Message})";
                    }
                }
                else if (pendingPurchase.PaymentMethod.Contains("CREDIT") || pendingPurchase.PaymentMethod.Contains("DEBIT"))
                {
                    response.Mensagem = "Pagamento com cartão em processamento ou aguardando ação.";
                }
            }
        }
        else if (request.TipoCompra == "ANUAL")
        {
            // Check if subscription is ALREADY ACTIVE
            var hasActiveSubscription = await _context.Subscriptions
                .AnyAsync(s => s.UserId == user.Id && s.Status == Nexas.Domain.Enums.SubscriptionStatus.Active, cancellationToken);
            
            if (hasActiveSubscription)
            {
                response.JaPago = true;
                return response;
            }

            var pendingSubscriptionPayment = await _context.SubscriptionPayments
                .Include(p => p.Subscription)
                .Where(p => p.Subscription.UserId == user.Id && p.Status == SubscriptionPaymentStatus.Pending)
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (pendingSubscriptionPayment != null)
            {
                response.TemPendencia = true;
                response.Status = "PENDING";
                // Assuming PIX if it has AsaasPaymentId, since SubscriptionPayment has no PaymentMethod entity right now
                response.MetodoPagamento = "PIX";

                if (!string.IsNullOrEmpty(pendingSubscriptionPayment.AsaasPaymentId))
                {
                    try
                    {
                        var qrCodeData = await _asaasService.GetPixQrCodeAsync(pendingSubscriptionPayment.AsaasPaymentId, cancellationToken);
                        response.PixCopiaECola = qrCodeData.Payload;
                        response.QrCodeBase64 = qrCodeData.EncodedImage;
                        response.Mensagem = "Você já possui um PIX aguardando pagamento para sua assinatura.";
                    }
                    catch (Exception ex)
                    {
                        // Failed to retrieve QR Code from Asaas (might be expired, deleted, or API error)
                        // DO NOT cancel the payment. 
                        response.Mensagem = $"Houve um erro ao recuperar o QR Code do PIX. (Erro: {ex.Message})";
                    }
                }
            }
        }

        return response;
    }
}
