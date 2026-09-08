using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.ForumCategories.Queries;

namespace Nexas.Application.ForumCategories.Queries.GetForumCategories;

public record GetForumCategoriesQuery() : IRequest<List<ForumCategoryDto>>;

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
            .OrderBy(x => x.Name)
            .Select(x => new ForumCategoryDto(x.Id, x.Name, x.Description, x.Active, x.Icon))
            .ToListAsync(cancellationToken);
    }
}
