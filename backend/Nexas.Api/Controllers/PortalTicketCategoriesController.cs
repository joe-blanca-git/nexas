using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Nexas.Application.Tickets.Queries.GetActiveTicketCategories;

namespace Nexas.Api.Controllers;

[Authorize]
[Route("api/v1/ticket-categories")]
[Tags("Portal Pan - Tickets (Helpdesk)")]
public class PortalTicketCategoriesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public PortalTicketCategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lista as categorias ativas de suporte.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        return Ok(await _mediator.Send(new GetActiveTicketCategoriesQuery()));
    }
}
