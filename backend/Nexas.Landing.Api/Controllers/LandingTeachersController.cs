using Microsoft.AspNetCore.Mvc;
using Nexas.Application.Teachers.Queries.GetTeachers;
using Nexas.Application.Teachers.Queries.GetTeacherById;
using Nexas.Application.Teachers.Common;
using Microsoft.AspNetCore.Authorization;

namespace Nexas.Landing.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/v1/Teachers")]
public class LandingTeachersController : ApiControllerBase
{
    /// <summary>
    /// Lista todos os professores ativos.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<TeacherDto>>> GetTeachers()
    {
        return await Mediator.Send(new GetTeachersQuery(IsPublic: true));
    }

    /// <summary>
    /// Obtém detalhes de um professor pelo ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TeacherDto>> GetTeacherById(int id)
    {
        var teacher = await Mediator.Send(new GetTeacherByIdQuery { Id = id, IsPublic = true });
        if (teacher == null) return NotFound();
        return teacher;
    }
}
