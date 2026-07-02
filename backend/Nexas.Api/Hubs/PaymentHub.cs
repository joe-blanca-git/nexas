using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Nexas.Api.Hubs;

[Authorize]
public class PaymentHub : Hub
{
    // Hub vazio pois as mensagens serão emitidas apenas do servidor para o cliente
}
