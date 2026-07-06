using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.Portal.Forum.Topics.Commands.CreateForumTopic;
using Nexas.Application.Portal.Forum.Topics.Commands.ResolveForumTopic;
using Nexas.Application.Portal.Forum.Topics.Commands.ReopenForumTopic;
using Nexas.Application.Portal.Forum.Topics.Queries.GetForumTopics;
using Nexas.Application.Portal.Forum.Topics.Queries.GetForumTopicById;
using Nexas.Application.Portal.Forum.Messages.Commands.ReplyForumTopic;

namespace Nexas.Api.Controllers;

[Authorize]
[Route("api/v1/[controller]")]
[Tags("Fórum - Tópicos")]
public class ForumTopicsController : ApiControllerBase
{
    /// <summary>
    /// Lista os tópicos do fórum com base nos filtros informados.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetForumTopicsQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Obtém os detalhes completos de um tópico pelo ID, incluindo mensagens.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await Mediator.Send(new GetForumTopicByIdQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Cria um novo tópico no fórum.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateForumTopicCommand command)
    {
        var result = await Mediator.Send(command);
        return Created(string.Empty, new { id = result });
    }

    /// <summary>
    /// Adiciona uma mensagem/resposta a um tópico existente.
    /// </summary>
    [HttpPost("{id}/reply")]
    public async Task<IActionResult> Reply(int id, [FromBody] ReplyForumTopicCommand command)
    {
        if (id != command.TopicId)
            return BadRequest("O ID do tópico informado na rota é diferente do ID da requisição.");

        var result = await Mediator.Send(command);
        return Created(string.Empty, new { id = result });
    }

    /// <summary>
    /// Marca um tópico como resolvido.
    /// </summary>
    [HttpPatch("{id}/resolve")]
    public async Task<IActionResult> Resolve(int id)
    {
        await Mediator.Send(new ResolveForumTopicCommand(id));
        return NoContent();
    }

    /// <summary>
    /// Reabre um tópico resolvido.
    /// </summary>
    [HttpPatch("{id}/reopen")]
    public async Task<IActionResult> Reopen(int id)
    {
        await Mediator.Send(new ReopenForumTopicCommand(id));
        return NoContent();
    }
}
