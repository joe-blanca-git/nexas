using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Nexas.Application.Tickets.Queries.GetActiveTicketCategories;

namespace Nexas.Admin.Api.Controllers;

[Authorize(Roles = "Teacher")]
[Route("api/v1/ticket-categories")]
[Tags("Admin - Tickets (Helpdesk)")]
[ApiController]
public class TicketCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TicketCategoriesController(IMediator mediator)
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
