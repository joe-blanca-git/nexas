using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.Checkout.Commands;

namespace Nexas.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/financeiro/checkout")]
public class FinancialCheckoutController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public FinancialCheckoutController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("pix")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CheckoutPixResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckoutPix([FromBody] CheckoutPixRequestDto request)
    {
        try
        {
            var command = new ProcessPixCheckoutCommand { Request = request };
            var result = await _mediator.Send(command);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            // O pipeline global de exceptions do clean architecture irá capturar ValidationException e outras e formatar.
            // Em caso de erro nativo da API externa:
            return BadRequest(new { Message = "Erro ao processar checkout PIX", Detalhe = ex.Message });
        }
    }
}
