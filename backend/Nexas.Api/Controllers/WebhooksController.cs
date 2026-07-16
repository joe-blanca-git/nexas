using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nexas.Application.Common.Models.Bunny;
using Nexas.Application.Webhooks.Commands.ProcessBunnyWebhook;
using System.Threading.Tasks;

namespace Nexas.Api.Controllers
{
    [ApiController]
    [Route("api/v1/webhooks")]
    [Tags("Webhooks Externos")]
    public class WebhooksController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;

        public WebhooksController(IMediator mediator, IConfiguration configuration)
        {
            _mediator = mediator;
            _configuration = configuration;
        }

        [HttpPost("bunny")]
        public async Task<IActionResult> BunnyWebhook([FromBody] BunnyWebhookPayload payload)
        {
            // CRÍTICO: Validar a assinatura e origem do webhook conforme o plano
            // A Bunny Stream permite passar parâmetros de segurança via Header ou QueryString na configuração do Webhook
            // Recomendação: Configurar a URL do webhook na Bunny como https://api.nexas.com/api/v1/webhooks/bunny?secret=YOUR_SECRET
            // ou validar um cabeçalho customizado.
            
            var configuredSecret = _configuration["BunnyNets:WebhookSecret"];
            if (string.IsNullOrEmpty(configuredSecret))
            {
                // Se não houver secret configurado no sistema, rejeitamos a requisição por segurança
                return Unauthorized(new { message = "WebhookSecret não configurado no servidor." });
            }

            // Exemplo de validação via Header (se configurado na Bunny para enviar X-Bunny-Signature)
            var hasHeader = Request.Headers.TryGetValue("X-Bunny-Signature", out var headerSignature);
            
            // Exemplo de validação via Query String
            var hasQuery = Request.Query.TryGetValue("secret", out var querySecret);

            bool isValid = false;

            if (hasHeader && headerSignature.ToString() == configuredSecret)
                isValid = true;
                
            if (hasQuery && querySecret.ToString() == configuredSecret)
                isValid = true;

            if (!isValid)
            {
                return Unauthorized(new { message = "Assinatura do Webhook inválida." });
            }

            var command = new ProcessBunnyWebhookCommand { Payload = payload };
            var result = await _mediator.Send(command);

            if (!result)
            {
                // Retornar OK mesmo se a Lesson não for encontrada evita que a Bunny fique tentando reenviar o webhook
                return Ok(new { message = "Vídeo não encontrado ou não gerenciado." });
            }

            return Ok(new { message = "Webhook processado com sucesso." });
        }
    }
}
