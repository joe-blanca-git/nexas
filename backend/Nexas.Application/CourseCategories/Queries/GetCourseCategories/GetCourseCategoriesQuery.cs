using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.CourseCategories.Queries.GetCourseCategories;

public record GetCourseCategoriesQuery() : IRequest<List<CourseCategoryDto>>;

public class GetCourseCategoriesQueryHandler : IRequestHandler<GetCourseCategoriesQuery, List<CourseCategoryDto>>
{
    private readonly INexasDbContext _context;

    public GetCourseCategoriesQueryHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<List<CourseCategoryDto>> Handle(GetCourseCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _context.CourseCategories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CourseCategoryDto(x.Id, x.Name, x.Description, x.Active))
            .ToListAsync(cancellationToken);
    }
}
