using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.Common.Models;
using Nexas.Application.Tickets.DTOs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.Tickets.Queries.GetAdminTickets;

public class GetAdminTicketsQueryHandler : IRequestHandler<GetAdminTicketsQuery, PaginatedList<TicketAdminSummaryDto>>
{
    private readonly INexasDbContext _context;

    public GetAdminTicketsQueryHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<TicketAdminSummaryDto>> Handle(GetAdminTicketsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.User)
            .AsNoTracking();

        if (request.Status.HasValue)
            query = query.Where(t => t.Status == request.Status.Value);
        
        if (request.CategoryId.HasValue)
            query = query.Where(t => t.TicketCategoryId == request.CategoryId.Value);

        if (request.Priority.HasValue)
            query = query.Where(t => t.Priority == request.Priority.Value);

        if (request.StudentId.HasValue)
            query = query.Where(t => t.UserId == request.StudentId.Value);

        if (request.StartDate.HasValue)
            query = query.Where(t => t.CreatedAt >= request.StartDate.Value);
            
        if (request.EndDate.HasValue)
            query = query.Where(t => t.CreatedAt <= request.EndDate.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchText))
            query = query.Where(t => t.Subject.Contains(request.SearchText) || (t.User.FullName != null && t.User.FullName.Contains(request.SearchText)));

        if (request.NoReplyOnly.HasValue && request.NoReplyOnly.Value)
            query = query.Where(t => t.LastReplyAt == null);

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
                StudentName = t.User.FullName,
                StudentEmail = t.User.Email,
                t.CreatedAt,
                t.LastReplyAt
            })
            .ToListAsync(cancellationToken);

        var items = dbItems.Select(t => new TicketAdminSummaryDto
        {
            Id = t.Id,
            Subject = t.Subject,
            Status = ((int)t.Status).ToString(),
            Priority = t.Priority.ToString(),
            CategoryName = t.CategoryName,
            StudentName = t.StudentName ?? "Aluno",
            StudentEmail = t.StudentEmail ?? "",
            CreatedAt = t.CreatedAt,
            LastReplyAt = t.LastReplyAt
        }).ToList();

        return new PaginatedList<TicketAdminSummaryDto>(items, totalCount, request.PageIndex, request.PageSize);
    }
}
