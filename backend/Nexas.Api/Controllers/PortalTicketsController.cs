using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using Nexas.Application.Portal.Tickets.Commands.CreateTicket;
using Nexas.Application.Portal.Tickets.Commands.ReplyTicket;
using Nexas.Application.Portal.Tickets.Commands.UploadTicketAttachment;
using Nexas.Application.Portal.Tickets.Queries.GetTicketDetails;
using Nexas.Application.Portal.Tickets.Queries.GetTickets;
using Nexas.Application.Portal.Tickets.Queries.GetTicketTimeline;
using Nexas.Domain.Enums;

namespace Nexas.Api.Controllers;

[Authorize]
[Route("api/v1/tickets")]
[Tags("Portal Pan - Tickets (Helpdesk)")]
public class PortalTicketsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public PortalTicketsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Cria um novo ticket de suporte.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketCommand command)
    {
        var ticketId = await _mediator.Send(command);
        return Ok(new { TicketId = ticketId });
    }

    /// <summary>
    /// Lista os tickets do aluno com paginação e filtros.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTickets([FromQuery] TicketStatus? status, [FromQuery] int? categoryId, [FromQuery] string? searchText, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetTicketsQuery(status, categoryId, searchText, pageIndex, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Traz os detalhes completos de um ticket (mensagens, anexos).
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTicketDetails(int id)
    {
        var result = await _mediator.Send(new GetTicketDetailsQuery(id));
        if (result == null) return NotFound(new { message = "Ticket não encontrado." });
        return Ok(result);
    }

    /// <summary>
    /// Adiciona uma nova resposta do aluno ao ticket.
    /// </summary>
    [HttpPost("{id:int}/messages")]
    public async Task<IActionResult> ReplyTicket(int id, [FromBody] ReplyTicketCommand command)
    {
        if (id != command.TicketId) return BadRequest(new { message = "O ID da rota e do corpo não conferem." });
        var messageId = await _mediator.Send(command);
        if (messageId == 0) return NotFound(new { message = "Ticket não encontrado." });
        return Ok(new { MessageId = messageId });
    }

    /// <summary>
    /// Faz upload de anexo para uma resposta.
    /// </summary>
    [HttpPost("{id:int}/attachments")]
    public async Task<IActionResult> UploadAttachment(int id, [FromForm] int messageId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Arquivo não enviado." });

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var content = ms.ToArray();

        var command = new UploadTicketAttachmentCommand(id, messageId, file.FileName, file.ContentType, content);
        var attachmentId = await _mediator.Send(command);

        if (attachmentId == 0) return NotFound(new { message = "Ticket ou mensagem não encontrado." });
        return Ok(new { AttachmentId = attachmentId });
    }

    /// <summary>
    /// Lista todo o histórico de eventos/auditoria do ticket.
    /// </summary>
    [HttpGet("{id:int}/timeline")]
    public async Task<IActionResult> GetTimeline(int id)
    {
        var result = await _mediator.Send(new GetTicketTimelineQuery(id));
        return Ok(result);
    }
}
