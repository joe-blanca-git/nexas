using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.Portal.Financial.Queries.GetMyTransactions;

namespace Nexas.Lemon.Ead.Api.Controllers;

[Authorize]
[Route("api/v1/financial")]
[Tags("Portal Pan - Financeiro")]
public class PortalFinancialController : ApiControllerBase
{
    [HttpGet("transactions")]
    public async Task<IActionResult> GetMyTransactions()
    {
        var result = await Mediator.Send(new GetMyTransactionsQuery());
        return Ok(result);
    }
}
