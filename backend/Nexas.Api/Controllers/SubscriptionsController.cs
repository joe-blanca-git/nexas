using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.Subscriptions.Commands;
using Nexas.Application.Subscriptions.Queries.GetMySubscription;

namespace Nexas.Api.Controllers;

/// <summary>
/// Controller responsável pelas operações de assinatura recorrente no Painel do Aluno (PAN).
/// </summary>
[Authorize]
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class SubscriptionsController : ApiControllerBase
{
    /// <summary>
    /// Inicia o processo de assinatura recorrente.
    /// </summary>
    /// <remarks>
    /// Exemplo de request:
    /// {
    ///   "planName": "Premium",
    ///   "amount": 99.90
    /// }
    /// </remarks>
    /// <param name="command">Dados da assinatura (nome do plano, valor).</param>
    /// <returns>ID da assinatura gerada no sistema.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> Create([FromBody] CreateSubscriptionCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(Create), new { id = result }, result);
    }

    /// <summary>
    /// Cancela uma assinatura recorrente.
    /// </summary>
    /// <remarks>
    /// O cancelamento só é permitido dentro do período de trial (até 7 dias).
    /// </remarks>
    /// <param name="subscriptionId">ID da assinatura a ser cancelada.</param>
    /// <returns>Resultado da operação de cancelamento.</returns>
    [HttpPost("{subscriptionId}/cancel")]
    [ProducesResponseType(typeof(Nexas.Application.Subscriptions.Commands.CancelSubscription.CancelSubscriptionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Cancel([FromRoute] int subscriptionId)
    {
        var result = await Mediator.Send(new Nexas.Application.Subscriptions.Commands.CancelSubscription.CancelSubscriptionCommand(subscriptionId));
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Obtém a assinatura ativa do usuário autenticado.
    /// </summary>
    /// <remarks>
    /// Retorna detalhes da assinatura atual ou 404 caso não exista.
    /// </remarks>
    /// <returns>Detalhes da assinatura do usuário.</returns>
    [HttpGet("my-subscription")]
    [ProducesResponseType(typeof(UserSubscriptionDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMySubscription()
    {
        var result = await Mediator.Send(new GetMySubscriptionQuery());
        if (result == null)
            return NotFound(new { message = "Assinatura não encontrada." });
        return Ok(result);
    }
}
