using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.Checkout.Commands;
using Nexas.Application.Purchases.Commands;

namespace Nexas.Admin.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/financeiro/checkout")]
public class FinancialCheckoutController : ApiControllerBase
{
    [HttpPost("pix")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CheckoutPixResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckoutPix([FromBody] CheckoutPixRequestDto request)
    {
        try
        {
            if (request.TipoCompra?.ToUpper() == "AVULSO")
            {
                var command = new CreatePurchaseCommand(request.CursoId, request.Valor, "PIX", request.Cpf);
                var result = await Mediator.Send(command);
                return Ok(new CheckoutPixResponseDto
                {
                    Sucesso = true,
                    CobrancaId = result.AsaasPaymentId,
                    PixCopiaECola = result.PixCopyPaste ?? string.Empty,
                    QrCode = result.PixQrCode ?? string.Empty
                });
            }

            return BadRequest(new { Message = "Tipo de compra inválido." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = "Erro ao processar checkout PIX", Detalhe = ex.Message });
        }
    }

    [HttpGet("pendencias")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Nexas.Application.Checkout.Queries.CheckoutPendenciasResponseDto))]
    public async Task<IActionResult> GetPendencias([FromQuery] int? cursoId, [FromQuery] string tipoCompra)
    {
        var query = new Nexas.Application.Checkout.Queries.GetCheckoutPendenciasQuery
        {
            CursoId = cursoId,
            TipoCompra = tipoCompra ?? "AVULSO"
        };

        var result = await Mediator.Send(query);
        return Ok(result);
    }
}
