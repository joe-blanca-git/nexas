using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.Courses.Commands.GenerateLessonVideoUpload;
using Nexas.Application.Courses.Commands.CompleteLessonVideoUpload;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.Courses.Commands.CreateCourse;
using Nexas.Application.Courses.Commands.CreateModule;
using Nexas.Application.Courses.Commands.UpdateModule;
using Nexas.Application.Courses.Commands.DeleteModule;
using Nexas.Application.Courses.Commands.DeactivateModule;
using Nexas.Application.Courses.Commands.CreateLesson;
using Nexas.Application.Courses.Commands.UpdateLesson;
using Nexas.Application.Courses.Commands.DeleteLesson;
using Nexas.Application.Courses.Commands.DeactivateLesson;
using Nexas.Application.Courses.Commands.UpdateCourse;
using Nexas.Application.Courses.Commands.DeactivateCourse;
using Nexas.Application.Courses.Queries.GetCourses;
using Nexas.Application.Courses.Queries.GetCourseById;
using Nexas.Application.Courses.Commands.CreateCourseDomain;
using Nexas.Application.Courses.Commands.UpdateCourseDomain;
using Nexas.Application.Courses.Commands.DeleteCourseDomain;
using Swashbuckle.AspNetCore.Annotations;

namespace Nexas.Admin.Api.Controllers
{
    /// <summary>
    /// Gerenciamento de Cursos, Módulos e Aulas.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("v1/api/[controller]")]
    [Produces("application/json")]
    public class CoursesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CoursesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region Courses

        /// <summary>
        /// Obtém a lista de todos os cursos ativos com módulos e aulas.
        /// </summary>
        /// <remarks>
        /// Este endpoint é público e não requer autenticação. Retorna a hierarquia completa: 
        /// cursos → módulos → aulas com todos os detalhes.
        /// Usado principalmente pela Landing Page e páginas de detalhes de cursos.
        /// </remarks>
        [AllowAnonymous]
        [HttpGet]
        [SwaggerOperation(Summary = "Lista cursos com módulos e aulas", Description = "Retorna uma lista completa de cursos ativos com todos os módulos e aulas inclusos.")]
        [ProducesResponseType(typeof(List<CourseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetCoursesQuery(IncludeInactive: true, FilterByCurrentUserTeacher: true));
            return Ok(result);
        }

        /// <summary>
        /// Obtém um curso específico pelo ID com detalhes.
        /// </summary>
        [Authorize(Roles = "Teacher")]
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Obtém um curso", Description = "Retorna os detalhes de um curso ativo específico pelo ID, incluindo módulos e aulas. Restrito aos cursos do professor.")]
        [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetCourseByIdQuery { Id = id, IncludeInactive = true, FilterByCurrentUserTeacher = true });
            if (result == null)
                return NotFound(new { message = $"Curso com ID {id} não encontrado." });

            return Ok(result);
        }

        /// <summary>
        /// Cria um novo curso completo.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite a criação de um curso junto com seus módulos e aulas em uma única transação.
        /// Requer permissão de 'Teacher'.
        /// </remarks>
        /// <param name="command">Dados do curso, módulos e aulas.</param>
        /// <returns>O ID do curso criado.</returns>
        [Authorize(Roles = "Teacher")]
        [HttpPost]
        [SwaggerOperation(Summary = "Cria um novo curso", Description = "Cria a estrutura completa de um curso no banco de dados.")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateCourseCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result }, new { message = "Curso criado com sucesso!", id = result });
        }

        /// <summary>
        /// Atualiza os dados de um curso existente.
        /// </summary>
        /// <remarks>
        /// Permite atualizar nome, descrição, nível e preço de um curso.
        /// Requer permissão de 'Teacher'.
        /// </remarks>
        /// <param name="command">Dados a serem atualizados.</param>
        [Authorize(Roles = "Teacher")]
        [HttpPut]
        [SwaggerOperation(Summary = "Atualiza um curso", Description = "Atualiza os dados de um curso existente.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromBody] UpdateCourseCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Inativa um curso sem deletá-lo.
        /// </summary>
        /// <remarks>
        /// Marca o curso como inativo. Requer permissão de 'Teacher'.
        /// </remarks>
        /// <param name="command">Dados com o ID do curso a ser inativado.</param>
        [Authorize(Roles = "Teacher")]
        [HttpPatch("deactivate")]
        [SwaggerOperation(Summary = "Inativa um curso", Description = "Marca um curso como inativo sem deletá-lo do banco de dados.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Deactivate([FromBody] DeactivateCourseCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Atualiza as categorias de um curso.
        /// </summary>
        [Authorize(Roles = "Teacher")]
        [HttpPatch("{id}/categories")]
        [SwaggerOperation(Summary = "Atualiza categorias do curso", Description = "Remove categorias antigas e vincula as novas informadas no array.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AssignCategories(int id, [FromBody] List<int> categoryIds)
        {
            var command = new Nexas.Application.Courses.Commands.AssignCategories.AssignCourseCategoriesCommand(id, categoryIds);
            await _mediator.Send(command);
            return NoContent();
        }

        #endregion

        #region Modules

        /// <summary>
        /// Cria um novo módulo em um curso existente.
        /// </summary>
        /// <remarks>
        /// Permite adicionar um novo módulo a um curso já existente.
        /// Requer permissão de 'Teacher'.
        /// </remarks>
        /// <param name="command">Dados do módulo a ser criado.</param>
        /// <returns>O ID do módulo criado.</returns>
        [Authorize(Roles = "Teacher")]
        [HttpPost("modules")]
        [SwaggerOperation(Summary = "Cria um novo módulo", Description = "Cria um novo módulo em um curso existente.")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateModule([FromBody] CreateModuleCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result }, new { message = "Módulo criado com sucesso!", id = result });
        }

        /// <summary>
        /// Atualiza os dados de um módulo existente.
        /// </summary>
        /// <remarks>
        /// Permite atualizar nome e descrição de um módulo.
        /// Requer permissão de 'Teacher'.
        /// </remarks>
        /// <param name="command">Dados do módulo a serem atualizados.</param>
        [Authorize(Roles = "Teacher")]
        [HttpPut("modules")]
        [SwaggerOperation(Summary = "Atualiza um módulo", Description = "Atualiza os dados de um módulo existente.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateModule([FromBody] UpdateModuleCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Inativa um módulo sem deletá-lo.
        /// </summary>
        /// <remarks>
        /// Marca o módulo como inativo. Requer permissão de 'Teacher'.
        /// </remarks>
        /// <param name="command">Dados com o ID do módulo a ser inativado.</param>
        [Authorize(Roles = "Teacher")]
        [HttpPatch("modules/deactivate")]
        [SwaggerOperation(Summary = "Inativa um módulo", Description = "Marca um módulo como inativo sem deletá-lo do banco de dados.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeactivateModule([FromBody] DeactivateModuleCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Deleta permanentemente um módulo e todas as suas aulas.
        /// </summary>
        /// <remarks>
        /// Remove o módulo do banco de dados junto com todas as aulas associadas.
        /// Requer permissão de 'Teacher'.
        /// </remarks>
        /// <param name="id">ID do módulo a ser deletado.</param>
        [Authorize(Roles = "Teacher")]
        [HttpDelete("modules/{id}")]
        [SwaggerOperation(Summary = "Deleta um módulo", Description = "Remove permanentemente um módulo e todas as suas aulas.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteModule(int id)
        {
            await _mediator.Send(new DeleteModuleCommand { Id = id });
            return NoContent();
        }

        #endregion

        #region Lessons

        /// <summary>
        /// Cria uma nova aula em um módulo existente.
        /// </summary>
        /// <remarks>
        /// Permite adicionar uma nova aula a um módulo já existente.
        /// Requer permissão de 'Teacher'.
        /// </remarks>
        /// <param name="command">Dados da aula a ser criada.</param>
        /// <returns>O ID da aula criada.</returns>
        [Authorize(Roles = "Teacher")]
        [HttpPost("lessons")]
        [SwaggerOperation(Summary = "Cria uma nova aula", Description = "Cria uma nova aula em um módulo existente.")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateLesson([FromBody] CreateLessonCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result }, new { message = "Aula criada com sucesso!", id = result });
        }

        /// <summary>
        /// Gera as credenciais e URL para upload direto de vídeo da aula para a Bunny Stream.
        /// </summary>
        [Authorize(Roles = "Teacher")]
        [HttpPost("lessons/{lessonId}/video")]
        [SwaggerOperation(Summary = "Gera credenciais de upload de vídeo para a aula")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GenerateVideoUpload([FromRoute] int lessonId)
        {
            var command = new GenerateLessonVideoUploadCommand { LessonId = lessonId };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Sinaliza que o frontend concluiu o envio do vídeo para a Bunny Stream.
        /// </summary>
        [Authorize(Roles = "Teacher")]
        [HttpPost("lessons/{lessonId}/video/complete")]
        [SwaggerOperation(Summary = "Informa conclusão de upload de vídeo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CompleteVideoUpload([FromRoute] int lessonId)
        {
            var command = new CompleteLessonVideoUploadCommand { LessonId = lessonId };
            await _mediator.Send(command);
            return Ok(new { message = "Upload finalizado. O vídeo está em processamento." });
        }

        /// <summary>
        /// Atualiza os dados de uma aula existente.
        /// </summary>
        /// <remarks>
        /// Permite atualizar nome, descrição e duração de uma aula.
        /// Requer permissão de 'Teacher'.
        /// </remarks>
        /// <param name="command">Dados da aula a serem atualizados.</param>
        [Authorize(Roles = "Teacher")]
        [HttpPut("lessons")]
        [SwaggerOperation(Summary = "Atualiza uma aula", Description = "Atualiza os dados de uma aula existente.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateLesson([FromBody] UpdateLessonCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Inativa uma aula sem deletá-la.
        /// </summary>
        /// <remarks>
        /// Marca a aula como inativa. Requer permissão de 'Teacher'.
        /// </remarks>
        /// <param name="command">Dados com o ID da aula a ser inativada.</param>
        [Authorize(Roles = "Teacher")]
        [HttpPatch("lessons/deactivate")]
        [SwaggerOperation(Summary = "Inativa uma aula", Description = "Marca uma aula como inativa sem deletá-la do banco de dados.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeactivateLesson([FromBody] DeactivateLessonCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Deleta permanentemente uma aula.
        /// </summary>
        /// <remarks>
        /// Remove a aula do banco de dados.
        /// Requer permissão de 'Teacher'.
        /// </remarks>
        /// <param name="id">ID da aula a ser deletada.</param>
        [Authorize(Roles = "Teacher")]
        [HttpDelete("lessons/{id}")]
        [SwaggerOperation(Summary = "Deleta uma aula", Description = "Remove permanentemente uma aula.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            await _mediator.Send(new DeleteLessonCommand { Id = id });
            return NoContent();
        }

        #endregion

        #region CourseDomains

        /// <summary>
        /// Cria um novo domínio/benefício para um curso.
        /// </summary>
        [Authorize(Roles = "Teacher")]
        [HttpPost("{courseId}/domains")]
        [SwaggerOperation(Summary = "Cria um novo domínio/benefício", Description = "Cria um novo domínio/benefício associado a um curso existente.")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateCourseDomain(int courseId, [FromBody] CreateCourseDomainCommand command)
        {
            if (courseId != command.CourseId)
                return BadRequest(new { message = "O CourseId da URL não corresponde ao do corpo da requisição." });

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result }, new { message = "Domínio criado com sucesso!", id = result });
        }

        /// <summary>
        /// Atualiza os dados de um domínio/benefício existente.
        /// </summary>
        [Authorize(Roles = "Teacher")]
        [HttpPut("{courseId}/domains/{id}")]
        [SwaggerOperation(Summary = "Atualiza um domínio", Description = "Atualiza os dados de um domínio existente de um curso.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateCourseDomain(int courseId, int id, [FromBody] UpdateCourseDomainCommand command)
        {
            if (courseId != command.CourseId || id != command.Id)
                return BadRequest(new { message = "Os IDs da URL não correspondem aos do corpo da requisição." });

            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Deleta permanentemente um domínio de um curso.
        /// </summary>
        [Authorize(Roles = "Teacher")]
        [HttpDelete("{courseId}/domains/{id}")]
        [SwaggerOperation(Summary = "Deleta um domínio", Description = "Remove permanentemente um domínio de um curso.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteCourseDomain(int courseId, int id)
        {
            await _mediator.Send(new DeleteCourseDomainCommand { CourseId = courseId, Id = id });
            return NoContent();
        }

        #endregion
    }
}

