using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Portal.Courses.Queries.GetMyCourses;

public class GetMyCoursesQueryHandler : IRequestHandler<GetMyCoursesQuery, List<PortalMyCourseDto>>
{
    private readonly INexasDbContext _context;
    private readonly IUserContextService _userContextService;

    public GetMyCoursesQueryHandler(INexasDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<List<PortalMyCourseDto>> Handle(GetMyCoursesQuery request, CancellationToken cancellationToken)
    {
        var user = await _userContextService.GetCurrentUserAsync();
        
        var enrolledCourseIds = await _context.Enrollments
            .Where(e => e.UserId == user.Id && e.Active)
            .Select(e => e.CourseId)
            .ToListAsync(cancellationToken);
            
        var courses = await _context.Courses
            .AsNoTracking()
            .Where(c => c.Active)
            .Select(c => new {
                Course = c,
                Categories = c.CourseCategories.Select(cc => cc.Category).ToList(),
                Released = enrolledCourseIds.Contains(c.Id),
                TotalLessons = c.Modules.SelectMany(m => m.Lessons).Count(l => l.Active),
                CompletedLessons = c.Modules.SelectMany(m => m.Lessons).SelectMany(l => l.LessonViews).Count(lv => lv.UserId == user.Id),
                AverageRating = c.CourseRates.Any() ? c.CourseRates.Average(r => (double)r.Rate) : 0.0
            })
            .OrderByDescending(x => x.Released)
            .ThenByDescending(x => x.Course.CreatedAt)
            .ToListAsync(cancellationToken);
            
        var result = courses.Select(x => {
            int progress = x.TotalLessons > 0 ? (int)((x.CompletedLessons * 100.0) / x.TotalLessons) : 0;
            return new PortalMyCourseDto(
                x.Course.Id,
                x.Course.Name,
                x.Course.Description,
                x.Course.ImgCoverLink,
                x.Released,
                "#6366f1", // Color (A tabela de curso ainda não tem coluna Color)
                $"NX-{x.Course.Id:D4}", // Code (Mock, pois também não há coluna nativa)
                (decimal)Math.Round(x.AverageRating, 1),
                progress,
                x.CompletedLessons,
                x.TotalLessons,
                x.Categories.Select(cat => new Nexas.Application.Courses.Common.CourseCategoryBasicDto(cat.Id, cat.Name)).ToList()
            );
        }).ToList();
        
        return result;
    }
}
