using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.Purchases.Commands;

namespace Nexas.Api.Controllers;

/// <summary>
/// Controller responsável pelas operações de compra no Painel do Aluno (PAN).
/// </summary>
[Authorize]
public class PurchasesController : ApiControllerBase
{
    /// <summary>
    /// Inicia o processo de compra de um curso (postComprarCurso).
    /// </summary>
    /// <param name="command">Dados da compra (CourseId, Valor, Método)</param>
    /// <returns>ID da compra gerada no sistema</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> Create(CreatePurchaseCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(Create), new { id = result }, result);
    }
}