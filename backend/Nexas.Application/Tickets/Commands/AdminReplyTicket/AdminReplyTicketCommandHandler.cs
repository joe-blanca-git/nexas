using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;
using Nexas.Domain.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.Tickets.Commands.AdminReplyTicket;

public class AdminReplyTicketCommandHandler : IRequestHandler<AdminReplyTicketCommand, int>
{
    private readonly INexasDbContext _context;
    private readonly IUserContextService _userContextService;
    private readonly IEmailService _emailService;

    public AdminReplyTicketCommandHandler(INexasDbContext context, IUserContextService userContextService, IEmailService emailService)
    {
        _context = context;
        _userContextService = userContextService;
        _emailService = emailService;
    }

    public async Task<int> Handle(AdminReplyTicketCommand request, CancellationToken cancellationToken)
    {
        var admin = await _userContextService.GetCurrentUserAsync();

        var ticket = await _context.Tickets
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

        if (ticket == null) return 0;

        ticket.UpdateLastReply();
        if (ticket.Status == TicketStatus.Closed)
        {
            ticket.UpdateStatus(TicketStatus.Open);
            _context.TicketTimelines.Add(TicketTimeline.Create(ticket.Id, admin.Id, TicketTimelineEvent.Reopened, "Ticket reaberto via nova resposta do suporte."));
        }
        else if (ticket.Status == TicketStatus.Open)
        {
            ticket.UpdateStatus(TicketStatus.Pending);
            _context.TicketTimelines.Add(TicketTimeline.Create(ticket.Id, admin.Id, TicketTimelineEvent.StatusChanged, "Status alterado automaticamente para Pendente após resposta."));
        }

        var message = TicketMessage.Create(ticket.Id, admin.Id, TicketOrigin.Backoffice, request.Content, null);
        _context.TicketMessages.Add(message);

        _context.TicketTimelines.Add(TicketTimeline.Create(ticket.Id, admin.Id, TicketTimelineEvent.Replied, "Ticket respondido pelo suporte."));

        await _context.SaveChangesAsync(cancellationToken);

        await _emailService.SendTicketReplyAsync(ticket.Id, ticket.User.Email ?? "", ticket.User.FullName ?? "Aluno", request.Content);

        return message.Id;
    }
}
