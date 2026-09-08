using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.Tickets.DTOs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.Tickets.Queries.GetTicketStats;

public class GetTicketStatsQueryHandler : IRequestHandler<GetTicketStatsQuery, TicketStatsDto>
{
    private readonly INexasDbContext _context;

    public GetTicketStatsQueryHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<TicketStatsDto> Handle(GetTicketStatsQuery request, CancellationToken cancellationToken)
    {
        var categoryStats = await _context.Tickets
            .Include(t => t.Category)
            .GroupBy(t => t.Category.Description)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToDictionaryAsync(k => k.Category, v => v.Count, cancellationToken);

        var statusStats = await _context.Tickets
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(k => k.Status, v => v.Count, cancellationToken);

        return new TicketStatsDto
        {
            ByCategory = categoryStats,
            ByStatus = statusStats
        };
    }
}
