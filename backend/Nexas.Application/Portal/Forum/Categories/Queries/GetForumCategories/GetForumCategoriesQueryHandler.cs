using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Portal.Forum.Categories.Queries.GetForumCategories;

public class GetForumCategoriesQueryHandler : IRequestHandler<GetForumCategoriesQuery, List<ForumCategoryDto>>
{
    private readonly INexasDbContext _context;

    public GetForumCategoriesQueryHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<List<ForumCategoryDto>> Handle(GetForumCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _context.ForumCategories
            .AsNoTracking()
            .Select(c => new ForumCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Active = c.Active
            })
            .ToListAsync(cancellationToken);
    }
}
