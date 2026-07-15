using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.Courses.Queries.GetCourses;
using Nexas.Application.Courses.Queries.GetCourseById;
using Swashbuckle.AspNetCore.Annotations;

namespace Nexas.Landing.Api.Controllers
{
    /// <summary>
    /// Leitura pública de Cursos para a Landing Page.
    /// </summary>
    [AllowAnonymous]
    [ApiController]
    [Route("v1/api/Courses")]
    [Produces("application/json")]
    public class LandingCoursesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LandingCoursesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Lista cursos", Description = "Retorna uma lista pública de cursos ativos.")]
        [ProducesResponseType(typeof(List<CourseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetCoursesQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Obtém um curso por ID")]
        [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetCourseByIdQuery { Id = id });
            if (result == null)
                return NotFound(new { message = $"Curso com ID {id} não encontrado." });

            return Ok(result);
        }
    }
}
