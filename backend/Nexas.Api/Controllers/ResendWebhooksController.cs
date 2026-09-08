using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Nexas.Application.Tickets.Commands.ProcessResendWebhook;

namespace Nexas.Api.Controllers;

[AllowAnonymous]
[Route("api/v1/webhooks/resend")]
[Tags("Webhooks - Resend")]
public class ResendWebhooksController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public ResendWebhooksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Recebe eventos de e-mail inbound do Resend.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> HandleResendWebhook([FromBody] ProcessResendWebhookCommand command)
    {
        if (command == null || command.Data == null)
            return BadRequest(new { message = "Payload inválido." });

        // Apenas processa se for um evento de e-mail recebido
        if (command.Type == "email.received")
        {
            await _mediator.Send(command);
        }

        // Sempre retornamos 200 OK para o Resend entender que recebemos o webhook.
        return Ok(new { received = true });
    }
}
