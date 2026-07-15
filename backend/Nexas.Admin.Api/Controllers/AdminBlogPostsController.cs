using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.BlogPosts.Commands.CreateBlogPost;
using Nexas.Application.BlogPosts.Commands.UpdateBlogPost;
using Nexas.Application.BlogPosts.Commands.DeleteBlogPost;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Http;

namespace Nexas.Admin.Api.Controllers;

[Authorize(Roles = "Teacher")]
[Route("api/v1/BlogPosts")]
public class AdminBlogPostsController : ApiControllerBase
{
    [HttpPost]
    [SwaggerOperation(Summary = "Criar um novo blog post", Description = "Cria um novo post no blog e retorna o seu ID.")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBlogPostCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(Create), new { id = result }, result); // changed from GetById to Create because GetById is not here
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Atualizar um blog post", Description = "Atualiza um post existente no blog pelo seu ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBlogPostCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Deletar um blog post", Description = "Deleta um post existente no blog pelo seu ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteBlogPostCommand(id));
        return NoContent();
    }
}
