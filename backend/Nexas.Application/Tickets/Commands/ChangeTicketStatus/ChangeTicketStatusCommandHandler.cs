using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;
using Nexas.Domain.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.Tickets.Commands.ChangeTicketStatus;

public class ChangeTicketStatusCommandHandler : IRequestHandler<ChangeTicketStatusCommand, bool>
{
    private readonly INexasDbContext _context;
    private readonly IUserContextService _userContextService;
    private readonly IEmailService _emailService;

    public ChangeTicketStatusCommandHandler(INexasDbContext context, IUserContextService userContextService, IEmailService emailService)
    {
        _context = context;
        _userContextService = userContextService;
        _emailService = emailService;
    }

    public async Task<bool> Handle(ChangeTicketStatusCommand request, CancellationToken cancellationToken)
    {
        var admin = await _userContextService.GetCurrentUserAsync();
        var ticket = await _context.Tickets.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);
        if (ticket == null) return false;

        var oldStatus = ticket.Status;
        if (oldStatus == request.Status) return true;

        ticket.UpdateStatus(request.Status);

        var timelineEvent = request.Status == TicketStatus.Closed ? TicketTimelineEvent.Closed : TicketTimelineEvent.StatusChanged;
        _context.TicketTimelines.Add(TicketTimeline.Create(
            ticket.Id, 
            admin.Id, 
            timelineEvent, 
            "Status alterado de '{oldStatus}' para '{request.Status}'."
        ));

        await _context.SaveChangesAsync(cancellationToken);

        if (request.Status == TicketStatus.Closed)
        {
            await _emailService.SendTicketClosedAsync(ticket.Id, ticket.User.Email ?? "", ticket.User.FullName ?? "Aluno");
        }
        else if (oldStatus == TicketStatus.Closed)
        {
            await _emailService.SendTicketStatusChangedAsync(ticket.Id, ticket.User.Email ?? "", ticket.User.FullName ?? "Aluno", request.Status.ToString());
        }

        return true;
    }
}
