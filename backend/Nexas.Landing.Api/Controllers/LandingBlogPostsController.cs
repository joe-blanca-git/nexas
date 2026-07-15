using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.BlogPosts.Queries.GetBlogPostById;
using Nexas.Application.BlogPosts.Queries.GetBlogPosts;
using Nexas.Application.BlogPosts.DTOs;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Http;

namespace Nexas.Landing.Api.Controllers;

[AllowAnonymous]
[Route("api/v1/BlogPosts")]
public class LandingBlogPostsController : ApiControllerBase
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
}
