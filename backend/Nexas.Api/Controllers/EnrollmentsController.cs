using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.Purchases.Commands.RefundCourse;
using MediatR;

namespace Nexas.Api.Controllers;

[Authorize]
[Route("api/enrollments")]
[ApiExplorerSettings(IgnoreApi = true)]
public class EnrollmentsController : ApiControllerBase
{
    [HttpPost("{enrollmentId}/refund")]
    [ProducesResponseType(typeof(RefundCourseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refund([FromRoute] int enrollmentId)
    {
        var result = await Mediator.Send(new RefundCourseCommand(enrollmentId));
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
