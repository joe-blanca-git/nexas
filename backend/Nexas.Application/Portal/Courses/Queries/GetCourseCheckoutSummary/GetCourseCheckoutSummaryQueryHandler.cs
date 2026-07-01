using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Portal.Courses.Queries.GetCourseCheckoutSummary;

public class GetCourseCheckoutSummaryQueryHandler : IRequestHandler<GetCourseCheckoutSummaryQuery, PortalCourseCheckoutSummaryDto?>
{
    private readonly INexasDbContext _context;

    public GetCourseCheckoutSummaryQueryHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<PortalCourseCheckoutSummaryDto?> Handle(GetCourseCheckoutSummaryQuery request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .Where(c => c.Id == request.CourseId && c.Active)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.ImgCoverLink,
                c.Description,
                c.PriceSingle,
                Category = c.CourseCategories.Select(cc => cc.Category.Name).FirstOrDefault() ?? "Geral",
                TotalLessons = c.Modules.SelectMany(m => m.Lessons).Count(l => l.Active),
                TotalDurationSeconds = c.Modules.SelectMany(m => m.Lessons).Where(l => l.Active).Sum(l => l.DurationSeconds ?? 0)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (course == null)
            return null;

        // Carga horária total (arredondada para horas)
        int totalHours = course.TotalDurationSeconds > 0 
            ? (int)Math.Ceiling(course.TotalDurationSeconds / 3600.0) 
            : 0;

        return new PortalCourseCheckoutSummaryDto(
            course.Id,
            course.Name,
            course.ImgCoverLink,
            course.Description,
            course.TotalLessons,
            totalHours,
            course.Category,
            course.PriceSingle ?? 0,
            79.90m // Valor hardcoded temporário (assinatura não está no DB)
        );
    }
}
