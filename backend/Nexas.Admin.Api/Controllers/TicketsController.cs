using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Nexas.Application.Tickets.Commands.AdminReplyTicket;
using Nexas.Application.Tickets.Commands.ChangeTicketCategory;
using Nexas.Application.Tickets.Commands.ChangeTicketPriority;
using Nexas.Application.Tickets.Commands.ChangeTicketStatus;
using Nexas.Application.Tickets.Queries.GetActiveTicketCategories;
using Nexas.Application.Tickets.Queries.GetAdminTicketDetails;
using Nexas.Application.Tickets.Queries.GetAdminTickets;
using Nexas.Application.Tickets.Queries.GetTicketDashboard;
using Nexas.Application.Tickets.Queries.GetTicketStats;
using Nexas.Domain.Enums;

namespace Nexas.Admin.Api.Controllers;

[Authorize(Roles = "Teacher")]
[Route("api/v1/[controller]")]
[ApiController]
[Tags("Admin - Tickets (Helpdesk)")]
public class TicketsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TicketsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lista os tickets com paginação e filtros detalhados.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTickets([FromQuery] TicketStatus? status, [FromQuery] int? categoryId, [FromQuery] TicketPriority? priority, [FromQuery] int? studentId, [FromQuery] string? searchText, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] bool? noReplyOnly, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetAdminTicketsQuery(status, categoryId, priority, studentId, searchText, startDate, endDate, noReplyOnly, pageIndex, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Busca rápida por tickets (assunto ou nome do aluno).
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchTickets([FromQuery] string searchText, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetAdminTicketsQuery(null, null, null, null, searchText, null, null, null, pageIndex, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Traz todos os detalhes de um ticket (Histórico, Aluno, Mensagens, Anexos).
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTicketDetails(int id)
    {
        var result = await _mediator.Send(new GetAdminTicketDetailsQuery(id));
        if (result == null) return NotFound(new { message = "Ticket não encontrado." });
        return Ok(result);
    }



    /// <summary>
    /// Professor responde ao ticket. (Muda status para Open e avisa o aluno por e-mail).
    /// </summary>
    [HttpPost("{id:int}/messages")]
    public async Task<IActionResult> ReplyTicket(int id, [FromBody] AdminReplyTicketCommand command)
    {
        if (id != command.TicketId) return BadRequest(new { message = "O ID da rota e do corpo não conferem." });
        var messageId = await _mediator.Send(command);
        if (messageId == 0) return NotFound(new { message = "Ticket não encontrado." });
        return Ok(new { MessageId = messageId });
    }

    /// <summary>
    /// Altera o status do ticket (Open, Pending, Answered, Closed).
    /// </summary>
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeTicketStatusCommand command)
    {
        if (id != command.TicketId) return BadRequest(new { message = "ID inválido." });
        var success = await _mediator.Send(command);
        if (!success) return NotFound(new { message = "Ticket não encontrado." });
        return NoContent();
    }

    /// <summary>
    /// Altera a prioridade do ticket (Low, Normal, High, Critical).
    /// </summary>
    [HttpPut("{id:int}/priority")]
    public async Task<IActionResult> ChangePriority(int id, [FromBody] ChangeTicketPriorityCommand command)
    {
        if (id != command.TicketId) return BadRequest(new { message = "ID inválido." });
        var success = await _mediator.Send(command);
        if (!success) return NotFound(new { message = "Ticket não encontrado." });
        return NoContent();
    }

    /// <summary>
    /// Transfere o ticket para outra categoria.
    /// </summary>
    [HttpPut("{id:int}/category")]
    public async Task<IActionResult> ChangeCategory(int id, [FromBody] ChangeTicketCategoryCommand command)
    {
        if (id != command.TicketId) return BadRequest(new { message = "ID inválido." });
        var success = await _mediator.Send(command);
        if (!success) return NotFound(new { message = "Ticket ou Categoria não encontrado." });
        return NoContent();
    }

    /// <summary>
    /// Retorna KPIs da tela inicial (Tickets Abertos, Pendentes, Tempo Médio).
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _mediator.Send(new GetTicketDashboardQuery());
        return Ok(result);
    }

    /// <summary>
    /// Retorna dados agrupados (Ex: Quantidade por Categoria/Status) para Gráficos.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _mediator.Send(new GetTicketStatsQuery());
        return Ok(result);
    }
}
