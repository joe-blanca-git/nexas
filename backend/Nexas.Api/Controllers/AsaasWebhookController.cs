using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.Webhooks.Commands;
using System.Text.Json;

namespace Nexas.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/webhooks/asaas")]
public class AsaasWebhookController : ApiControllerBase
{
    // A rota do webhook geralmente não deve exigir [Authorize] porque o Asaas que faz a chamada de fora
    [HttpPost]
    public async Task<IActionResult> ReceiveWebhook([FromBody] JsonElement payload)
    {
        try
        {
            // Extrai as informações de evento e do payment.id
            var eventType = payload.GetProperty("event").GetString();
            var paymentId = payload.GetProperty("payment").GetProperty("id").GetString();

            if (string.IsNullOrEmpty(eventType) || string.IsNullOrEmpty(paymentId))
                return BadRequest("Payload inválido");

            var command = new ProcessAsaasWebhookCommand
            {
                Event = eventType,
                PaymentId = paymentId
            };

            // Envia para o Handler
            var result = await Mediator.Send(command);

            return Ok(new { success = result });
        }
        catch (KeyNotFoundException)
        {
            // Retorna 200 pro Asaas parar de tentar caso seja um payload diferente do padrão
            return Ok();
        }
        catch (Exception ex)
        {
            // Registrar log
            return StatusCode(500, ex.Message);
        }
    }
}
