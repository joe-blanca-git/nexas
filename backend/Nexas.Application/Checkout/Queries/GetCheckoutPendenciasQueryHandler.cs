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
            // Verifica se JÁ ESTÁ PAGO
            var jaPago = await _context.Purchases
                .AnyAsync(p => p.UserId == user.Id && p.CourseId == request.CursoId.Value && p.Status == PurchaseStatus.Approved, cancellationToken);
            
            if (jaPago)
            {
                response.JaPago = true;
                return response;
            }

            var pendencia = await _context.Purchases
                .Where(p => p.UserId == user.Id && p.CourseId == request.CursoId.Value && p.Status == PurchaseStatus.Pending)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (pendencia != null)
            {
                response.TemPendencia = true;
                response.Status = "PENDING";
                response.MetodoPagamento = pendencia.PaymentMethod;

                if (pendencia.PaymentMethod == "PIX" && !string.IsNullOrEmpty(pendencia.AsaasPaymentId))
                {
                    try
                    {
                        var qrCodeData = await _asaasService.GetPixQrCodeAsync(pendencia.AsaasPaymentId, cancellationToken);
                        response.PixCopiaECola = qrCodeData.Payload;
                        response.QrCodeBase64 = qrCodeData.EncodedImage;
                        response.Mensagem = "Você já possui um PIX aguardando pagamento para este item.";
                    }
                    catch
                    {
                        // Falha ao recuperar QR Code do Asaas (pode ter sido expirado ou deletado)
                        pendencia.Cancel();
                        await _context.SaveChangesAsync(cancellationToken);
                        
                        response.TemPendencia = false;
                        response.Status = null;
                        response.MetodoPagamento = null;
                    }
                }
                else if (pendencia.PaymentMethod.Contains("CREDIT") || pendencia.PaymentMethod.Contains("DEBIT"))
                {
                    response.Mensagem = "Pagamento com cartão em processamento ou aguardando ação.";
                }
            }
        }
        else if (request.TipoCompra == "ANUAL")
        {
            // Verifica se a assinatura JÁ ESTÁ ATIVA
            var assinaturaAtiva = await _context.Subscriptions
                .AnyAsync(s => s.UserId == user.Id && s.Status == Nexas.Domain.Enums.SubscriptionStatus.Active, cancellationToken);
            
            if (assinaturaAtiva)
            {
                response.JaPago = true;
                return response;
            }

            var pendencia = await _context.SubscriptionPayments
                .Include(p => p.Subscription)
                .Where(p => p.Subscription.UserId == user.Id && p.Status == SubscriptionPaymentStatus.Pending)
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (pendencia != null)
            {
                response.TemPendencia = true;
                response.Status = "PENDING";
                // Assumindo PIX se tiver AsaasPaymentId, já que SubscriptionPayment não tem PaymentMethod no momento
                response.MetodoPagamento = "PIX";

                if (!string.IsNullOrEmpty(pendencia.AsaasPaymentId))
                {
                    try
                    {
                        var qrCodeData = await _asaasService.GetPixQrCodeAsync(pendencia.AsaasPaymentId, cancellationToken);
                        response.PixCopiaECola = qrCodeData.Payload;
                        response.QrCodeBase64 = qrCodeData.EncodedImage;
                        response.Mensagem = "Você já possui um PIX aguardando pagamento para sua assinatura.";
                    }
                    catch
                    {
                        // Falha ao recuperar QR Code do Asaas (pode ter sido expirado ou deletado)
                        pendencia.Cancel();
                        await _context.SaveChangesAsync(cancellationToken);
                        
                        response.TemPendencia = false;
                        response.Status = null;
                        response.MetodoPagamento = null;
                    }
                }
            }
        }

        return response;
    }
}
