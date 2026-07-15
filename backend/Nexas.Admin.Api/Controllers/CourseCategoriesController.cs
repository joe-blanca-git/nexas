using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.CourseCategories.Commands.CreateCourseCategory;
using Nexas.Application.CourseCategories.Commands.UpdateCourseCategory;
using Nexas.Application.CourseCategories.Commands.DeleteCourseCategory;
using Nexas.Application.CourseCategories.Queries.GetCourseCategories;
using Nexas.Application.CourseCategories.Queries.GetCourseCategoryById;

namespace Nexas.Admin.Api.Controllers;

[Authorize]
[Route("api/v1/course-categories")]
[Tags("Course Categories")]
public class CourseCategoriesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CourseCategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _mediator.Send(new GetCourseCategoriesQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetCourseCategoryByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourseCategoryCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCourseCategoryCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteCourseCategoryCommand(id));
        return NoContent();
    }
}
