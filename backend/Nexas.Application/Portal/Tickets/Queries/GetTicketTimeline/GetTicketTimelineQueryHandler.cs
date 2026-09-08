using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.Portal.Tickets.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.Portal.Tickets.Queries.GetTicketTimeline;

public class GetTicketTimelineQueryHandler : IRequestHandler<GetTicketTimelineQuery, List<TicketTimelineDto>?>
{
    private readonly INexasDbContext _context;
    private readonly IUserContextService _userContextService;

    public GetTicketTimelineQueryHandler(INexasDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<List<TicketTimelineDto>?> Handle(GetTicketTimelineQuery request, CancellationToken cancellationToken)
    {
        var user = await _userContextService.GetCurrentUserAsync();

        var hasAccess = await _context.Tickets.AnyAsync(t => t.Id == request.TicketId && t.UserId == user.Id, cancellationToken);
        if (!hasAccess) return null;

        var timelines = await _context.TicketTimelines
            .Where(t => t.TicketId == request.TicketId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TicketTimelineDto
            {
                Id = t.Id,
                Event = t.Event.ToString(),
                Description = t.Description,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return timelines;
    }
}
