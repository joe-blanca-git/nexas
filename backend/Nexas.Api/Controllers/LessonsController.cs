using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.Lessons.Commands.ToggleLessonView;
using Swashbuckle.AspNetCore.Annotations;

namespace Nexas.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/lessons")]
public class LessonsController : ApiControllerBase
{
    [HttpPost("{id}/toggle-view")]
    [SwaggerOperation(Summary = "Alterna o status de conclusão da aula para o aluno autenticado (Toggle)")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public async Task<IActionResult> ToggleLessonView(int id)
    {
        var command = new ToggleLessonViewCommand(id);
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}
