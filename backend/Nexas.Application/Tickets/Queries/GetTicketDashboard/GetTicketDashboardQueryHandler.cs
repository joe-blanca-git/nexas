using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.Tickets.DTOs;
using Nexas.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.Tickets.Queries.GetTicketDashboard;

public class GetTicketDashboardQueryHandler : IRequestHandler<GetTicketDashboardQuery, TicketDashboardDto>
{
    private readonly INexasDbContext _context;

    public GetTicketDashboardQueryHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<TicketDashboardDto> Handle(GetTicketDashboardQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var tickets = await _context.Tickets.AsNoTracking().ToListAsync(cancellationToken);

        int open = tickets.Count(t => t.Status == TicketStatus.Open);
        int closed = tickets.Count(t => t.Status == TicketStatus.Closed);
        int ticketsToday = tickets.Count(t => t.CreatedAt >= today && t.CreatedAt < tomorrow);
        
        // Pending/Replied might be a combination of Open and who replied last (student vs admin). For simplicity, we just simulate or base on LastReplyAt != null
        int replied = tickets.Count(t => t.LastReplyAt.HasValue);
        int pending = open - replied; // Or any logic for pending

        double avgReply = 0;
        var respondedTickets = tickets.Where(t => t.LastReplyAt.HasValue).ToList();
        if (respondedTickets.Any())
        {
            avgReply = respondedTickets.Average(t => (t.LastReplyAt!.Value - t.CreatedAt).TotalHours);
        }

        double avgClose = 0;
        var closedTickets = tickets.Where(t => t.ClosedAt.HasValue).ToList();
        if (closedTickets.Any())
        {
            avgClose = closedTickets.Average(t => (t.ClosedAt!.Value - t.CreatedAt).TotalHours);
        }

        return new TicketDashboardDto
        {
            OpenTickets = open,
            ClosedTickets = closed,
            TicketsToday = ticketsToday,
            PendingTickets = pending < 0 ? 0 : pending,
            RepliedTickets = replied,
            AverageReplyTimeHours = Math.Round(avgReply, 2),
            AverageCloseTimeHours = Math.Round(avgClose, 2)
        };
    }
}
