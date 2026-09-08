using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.Portal.Forum.Topics.Commands.CreateForumTopic;
using Nexas.Application.Portal.Forum.Topics.Commands.ResolveForumTopic;
using Nexas.Application.Portal.Forum.Topics.Commands.ReopenForumTopic;
using Nexas.Application.Portal.Forum.Topics.Queries.GetForumTopics;
using Nexas.Application.Portal.Forum.Topics.Queries.GetForumTopicById;
using Nexas.Application.Portal.Forum.Messages.Commands.ReplyForumTopic;

namespace Nexas.Admin.Api.Controllers;

[Authorize(Roles = "Teacher")]
[Route("api/v1/support-forum")]
[Tags("Admin - Fórum")]
public class ForumTopicsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetForumTopicsQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await Mediator.Send(new GetForumTopicByIdQuery(id));
        return Ok(result);
    }

    [HttpPost("{id}/reply")]
    public async Task<IActionResult> Reply(int id, [FromBody] ReplyForumTopicCommand command)
    {
        // Caso o client envie sem o TopicId, forçamos
        if (command.TopicId == 0)
        {
            command = new ReplyForumTopicCommand(id, command.Content);
        }

        if (id != command.TopicId)
            return BadRequest("O ID do tópico informado na rota é diferente do ID da requisição.");

        var result = await Mediator.Send(command);
        return Created(string.Empty, new { id = result });
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateForumStatusRequest request)
    {
        if (request.Status == "Resolved" || request.Status == "Finalizado")
            await Mediator.Send(new ResolveForumTopicCommand(id));
        else
            await Mediator.Send(new ReopenForumTopicCommand(id));
            
        return NoContent();
    }
}

public class UpdateForumStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
