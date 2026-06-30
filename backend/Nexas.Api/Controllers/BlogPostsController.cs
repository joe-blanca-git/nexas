using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.BlogPosts.Commands.CreateBlogPost;
using Nexas.Application.BlogPosts.Commands.UpdateBlogPost;
using Nexas.Application.BlogPosts.Commands.DeleteBlogPost;
using Nexas.Application.BlogPosts.Queries.GetBlogPostById;
using Nexas.Application.BlogPosts.Queries.GetBlogPosts;
using Nexas.Application.BlogPosts.DTOs;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Http;

namespace Nexas.Api.Controllers;

[Authorize(Roles = "Teacher")]
[Route("api/v1/[controller]")]
public class BlogPostsController : ApiControllerBase
{
    [HttpPost]
    [SwaggerOperation(Summary = "Criar um novo blog post", Description = "Cria um novo post no blog e retorna o seu ID.")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBlogPostCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result }, result);
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

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obter um blog post pelo ID", Description = "Retorna os detalhes de um post específico pelo seu ID.")]
    [ProducesResponseType(typeof(BlogPostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await Mediator.Send(new GetBlogPostByIdQuery(id));
        return Ok(result);
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Listar todos os blog posts", Description = "Retorna uma lista contendo todos os posts do blog ordenados do mais recente para o mais antigo.")]
    [ProducesResponseType(typeof(List<BlogPostDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetBlogPostsQuery());
        return Ok(result);
    }
}
