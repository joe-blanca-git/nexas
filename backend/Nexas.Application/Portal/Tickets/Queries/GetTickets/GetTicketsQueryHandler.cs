using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.Common.Models;
using Nexas.Application.Portal.Tickets.DTOs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.Portal.Tickets.Queries.GetTickets;

public class GetTicketsQueryHandler : IRequestHandler<GetTicketsQuery, PaginatedList<TicketSummaryDto>>
{
    private readonly INexasDbContext _context;
    private readonly IUserContextService _userContextService;

    public GetTicketsQueryHandler(INexasDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<PaginatedList<TicketSummaryDto>> Handle(GetTicketsQuery request, CancellationToken cancellationToken)
    {
        var user = await _userContextService.GetCurrentUserAsync();

        var query = _context.Tickets
            .Include(t => t.Category)
            .Where(t => t.UserId == user.Id);

        if (request.Status.HasValue)
        {
            query = query.Where(t => t.Status == request.Status.Value);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(t => t.TicketCategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            query = query.Where(t => t.Subject.Contains(request.SearchText));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var dbItems = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new
            {
                t.Id,
                t.Subject,
                t.Status,
                t.Priority,
                CategoryName = t.Category.Description,
                t.CreatedAt,
                t.LastReplyAt
            })
            .ToListAsync(cancellationToken);

        var items = dbItems.Select(t => new TicketSummaryDto
        {
            Id = t.Id,
            Subject = t.Subject,
            Status = ((int)t.Status).ToString(),
            Priority = t.Priority.ToString(),
            CategoryName = t.CategoryName,
            CreatedAt = t.CreatedAt,
            LastReplyAt = t.LastReplyAt
        }).ToList();

        return new PaginatedList<TicketSummaryDto>(items, totalCount, request.PageIndex, request.PageSize);
    }
}
