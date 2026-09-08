using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.BlogPosts.Queries.GetBlogPostById;
using Nexas.Application.BlogPosts.Queries.GetBlogPosts;
using Nexas.Application.BlogPosts.Commands.CreateBlogPost;
using Nexas.Application.BlogPosts.Commands.UpdateBlogPost;
using Nexas.Application.BlogPosts.Commands.DeleteBlogPost;
using Nexas.Application.BlogPosts.DTOs;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Http;

namespace Nexas.Api.Controllers;

[Authorize(Roles = "Teacher")]
[Route("api/v1/[controller]")]
public class BlogPostsController : ApiControllerBase
{

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

    [HttpPost]
    [SwaggerOperation(Summary = "Criar um novo post", Description = "Cria uma nova publicação no blog.")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBlogPostCommand command)
    {
        var id = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Atualizar um post", Description = "Atualiza os dados de uma publicação existente.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBlogPostCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("O ID da rota não corresponde ao ID do corpo da requisição.");
        }

        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Excluir um post", Description = "Remove uma publicação do blog pelo ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteBlogPostCommand(id));
        return NoContent();
    }
}
