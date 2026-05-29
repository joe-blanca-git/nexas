using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.Subscriptions.Commands;

namespace Nexas.Api.Controllers;

/// <summary>
/// Controller responsável pelas operações de assinatura recorrente no Painel do Aluno (PAN).
/// </summary>
[Authorize]
public class SubscriptionsController : ApiControllerBase
{
    /// <summary>
    /// Inicia o processo de assinatura recorrente (postAssinarPlano).
    /// </summary>
    /// <param name="command">Dados da assinatura (Nome do Plano, Valor)</param>
    /// <returns>ID da assinatura gerada no sistema</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> Create(CreateSubscriptionCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(Create), new { id = result }, result);
    }
}
