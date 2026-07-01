using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.Portal.Courses.Queries.GetMyCourses;

namespace Nexas.Api.Controllers;

[Authorize]
[Route("api/v1/portal/courses")]
[Tags("Portal Pan - Cursos")]
public class PortalCoursesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public PortalCoursesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyCourses()
    {
        var result = await _mediator.Send(new GetMyCoursesQuery());
        return Ok(result);
    }
    [HttpGet("{id}/checkout-summary")]
    public async Task<IActionResult> GetCourseCheckoutSummary(int id)
    {
        var result = await _mediator.Send(new Nexas.Application.Portal.Courses.Queries.GetCourseCheckoutSummary.GetCourseCheckoutSummaryQuery(id));
        if (result == null)
            return NotFound(new { message = "Curso não encontrado ou inativo." });
            
        return Ok(result);
    }
}
