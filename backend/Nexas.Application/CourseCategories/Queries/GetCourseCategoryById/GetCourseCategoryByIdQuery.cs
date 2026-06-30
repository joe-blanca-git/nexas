using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

namespace Nexas.Application.CourseCategories.Queries.GetCourseCategoryById;

public record GetCourseCategoryByIdQuery(int Id) : IRequest<CourseCategoryDto?>;

public class GetCourseCategoryByIdQueryHandler : IRequestHandler<GetCourseCategoryByIdQuery, CourseCategoryDto?>
{
    private readonly INexasDbContext _context;

    public GetCourseCategoryByIdQueryHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<CourseCategoryDto?> Handle(GetCourseCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.CourseCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return null;
        }

        return new CourseCategoryDto(entity.Id, entity.Name, entity.Description, entity.Active);
    }
}
