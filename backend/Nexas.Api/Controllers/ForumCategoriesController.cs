using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.Portal.Forum.Categories.Commands.CreateForumCategory;
using Nexas.Application.Portal.Forum.Categories.Commands.UpdateForumCategory;
using Nexas.Application.Portal.Forum.Categories.Commands.DeactivateForumCategory;
using Nexas.Application.Portal.Forum.Categories.Queries.GetForumCategories;

namespace Nexas.Api.Controllers;

[Authorize]
[Route("api/v1/[controller]")]
[Tags("Fórum - Categorias")]
public class ForumCategoriesController : ApiControllerBase
{
    /// <summary>
    /// Lista todas as categorias do fórum.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetForumCategoriesQuery());
        return Ok(result);
    }

    /// <summary>
    /// Cria uma nova categoria no fórum.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateForumCategoryCommand command)
    {
        var result = await Mediator.Send(command);
        return Created(string.Empty, new { id = result });
    }

    /// <summary>
    /// Atualiza uma categoria existente.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateForumCategoryCommand command)
    {
        if (id != command.Id)
            return BadRequest("O ID informado na rota é diferente do ID do corpo da requisição.");

        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Desativa uma categoria.
    /// </summary>
    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await Mediator.Send(new DeactivateForumCategoryCommand(id));
        return NoContent();
    }
}
