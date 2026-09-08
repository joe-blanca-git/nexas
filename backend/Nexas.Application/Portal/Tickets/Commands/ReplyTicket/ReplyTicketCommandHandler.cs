using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;
using Nexas.Domain.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.Portal.Tickets.Commands.ReplyTicket;

public class ReplyTicketCommandHandler : IRequestHandler<ReplyTicketCommand, int>
{
    private readonly INexasDbContext _context;
    private readonly IUserContextService _userContextService;
    private readonly IEmailService _emailService;

    public ReplyTicketCommandHandler(INexasDbContext context, IUserContextService userContextService, IEmailService emailService)
    {
        _context = context;
        _userContextService = userContextService;
        _emailService = emailService;
    }

    public async Task<int> Handle(ReplyTicketCommand request, CancellationToken cancellationToken)
    {
        var user = await _userContextService.GetCurrentUserAsync();

        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == request.TicketId && t.UserId == user.Id, cancellationToken);

        if (ticket == null)
            return 0;

        if (ticket.Status == TicketStatus.Closed)
        {
            ticket.UpdateStatus(TicketStatus.Open);
            var reopenTimeline = TicketTimeline.Create(ticket.Id, user.Id, TicketTimelineEvent.Reopened, "Ticket reaberto via nova resposta.");
            _context.TicketTimelines.Add(reopenTimeline);
        }

        ticket.UpdateLastReply();

        var message = TicketMessage.Create(ticket.Id, user.Id, TicketOrigin.Portal, request.Content, null);
        _context.TicketMessages.Add(message);

        var timeline = TicketTimeline.Create(ticket.Id, user.Id, TicketTimelineEvent.Replied, "Ticket respondido pelo aluno.");
        _context.TicketTimelines.Add(timeline);

        await _context.SaveChangesAsync(cancellationToken);

        await _emailService.SendTicketReplyAsync(ticket.Id, user.Email ?? "", user.FullName ?? "Aluno", request.Content);

        return message.Id;
    }
}
