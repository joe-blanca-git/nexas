using Microsoft.AspNetCore.SignalR;
using Nexas.SystemManager.Hubs;
using Nexas.Application.Common.Interfaces;

namespace Nexas.SystemManager.Services;

public class PaymentEventPublisher : IPaymentEventPublisher
{
    private readonly IHubContext<PaymentHub> _hubContext;

    public PaymentEventPublisher(IHubContext<PaymentHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PublishPaymentConfirmedAsync(string externalUserId, string tipoCompra, int cursoId)
    {
        await _hubContext.Clients.User(externalUserId).SendAsync("PaymentConfirmed", new
        {
            sucesso = true,
            tipoCompra,
            cursoId
        });
    }

    public async Task PublishPaymentRefundedAsync(string externalUserId, string tipoCompra, int cursoId)
    {
        await _hubContext.Clients.User(externalUserId).SendAsync("PaymentRefunded", new
        {
            sucesso = false,
            tipoCompra,
            cursoId
        });
    }

    public async Task PublishPaymentCanceledAsync(string externalUserId, string tipoCompra, int cursoId)
    {
        await _hubContext.Clients.User(externalUserId).SendAsync("PaymentCanceled", new
        {
            sucesso = false,
            tipoCompra,
            cursoId
        });
    }
}
