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
    /// <remarks>
    /// Exemplo de request:
    /// {
    ///   "courseId": 1,
    ///   "amount": 197.90,
    ///   "paymentMethod": "CREDIT_CARD",
    ///   "card": {
    ///     "holderName": "JOEDER BLANCA TESTE",
    ///     "number": "4444444444444444",
    ///     "expiryMonth": "12",
    ///     "expiryYear": "2030",
    ///     "ccv": "123",
    ///     "holderCpfCnpj": "39395533870"
    ///   }
    /// }
    ///
    /// Exemplo de response (201):
    /// {
    ///   "purchaseId": 3,
    ///   "status": "CONFIRMED",
    ///   "pixQrCode": null,
    ///   "pixCopyPaste": null,
    ///   "asaasPaymentId": "pay_yiygnzqwo7syi2o3"
    /// }
    /// </remarks>
    /// <param name="command">Dados da compra (CourseId, Amount, PaymentMethod, Card)</param>
    /// <returns>Detalhes do pagamento criado</returns>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(Nexas.Application.Purchases.Commands.PurchaseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Nexas.Application.Purchases.Commands.PurchaseResponseDto>> Create(CreatePurchaseCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(Create), new { id = result.PurchaseId }, result);
    }

    /// <summary>
    /// Cancela uma compra e realiza o estorno (refund) dentro do período permitido (até 7 dias).
    /// </summary>
    /// <remarks>
    /// Exemplo de request: POST /api/purchases/123/refund
    /// </remarks>
    /// <param name="purchaseId">ID da compra a ser estornada.</param>
    /// <returns>Resultado da operação de refund.</returns>
    [HttpPost("{purchaseId}/refund")]
    [ProducesResponseType(typeof(Nexas.Application.Purchases.Commands.RefundCourse.RefundCourseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refund([FromRoute] int purchaseId)
    {
        var result = await Mediator.Send(new Nexas.Application.Purchases.Commands.RefundCourse.RefundCourseCommand(purchaseId));
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Retorna a lista de compras do usuário autenticado.
    /// </summary>
    /// <remarks>
    /// Endpoint: GET /api/purchases/my-purchases
    /// </remarks>
    /// <returns>Lista de objetos UserPurchaseDto contendo detalhes das compras.</returns>
    [HttpGet("my-purchases")]
    [ProducesResponseType(typeof(System.Collections.Generic.List<Nexas.Application.Purchases.Queries.GetMyPurchases.UserPurchaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyPurchases()
    {
        var result = await Mediator.Send(new Nexas.Application.Purchases.Queries.GetMyPurchases.GetMyPurchasesQuery());
        return Ok(result);
    }
}