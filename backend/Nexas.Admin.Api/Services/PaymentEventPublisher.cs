using Microsoft.AspNetCore.SignalR;
using Nexas.Admin.Api.Hubs;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Admin.Api.Services;

public class PaymentEventPublisher : IPaymentEventPublisher
{
    private readonly IHubContext<PaymentHub> _hubContext;

    public PaymentEventPublisher(IHubContext<PaymentHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PublishPaymentConfirmedAsync(string externalUserId, string tipoCompra, int cursoId)
    {
        // O SignalR usa nativamente o ClaimTypes.NameIdentifier como User Identifier caso configurado,
        // mas aqui vamos usar o externalUserId (id do clerk/auth provider) ou User Identifier configurado
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
}
