using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.ForumCategories.Commands.CreateForumCategory;
using Nexas.Application.ForumCategories.Commands.DeleteForumCategory;
using Nexas.Application.ForumCategories.Commands.UpdateForumCategory;
using Nexas.Application.ForumCategories.Queries;
using Nexas.Application.ForumCategories.Queries.GetForumCategories;
using Nexas.Application.ForumCategories.Queries.GetForumCategoryById;

namespace Nexas.Admin.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ForumCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ForumCategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<ForumCategoryDto>>> Get()
    {
        return await _mediator.Send(new GetForumCategoriesQuery());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ForumCategoryDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetForumCategoryByIdQuery(id));

        if (result == null)
            return NotFound();

        return result;
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] CreateForumCategoryCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateForumCategoryCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("Id in route does not match Id in body.");
        }

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteForumCategoryCommand(id));
        return NoContent();
    }
}
