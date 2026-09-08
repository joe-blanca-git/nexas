using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.Tickets.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.Tickets.Queries.GetActiveTicketCategories;

public class GetActiveTicketCategoriesQueryHandler : IRequestHandler<GetActiveTicketCategoriesQuery, List<TicketCategoryDto>>
{
    private readonly INexasDbContext _context;

    public GetActiveTicketCategoriesQueryHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<List<TicketCategoryDto>> Handle(GetActiveTicketCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _context.TicketCategories
            .Where(c => c.Active)
            .Select(c => new TicketCategoryDto
            {
                Id = c.Id,
                Description = c.Description,
                Icon = c.Icon
            })
            .ToListAsync(cancellationToken);
    }
}
