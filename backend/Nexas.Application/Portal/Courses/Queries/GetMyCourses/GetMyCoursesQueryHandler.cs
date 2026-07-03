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
            .Include(c => c.CourseCategories)
                .ThenInclude(cc => cc.Category)
            .Where(c => c.Active)
            .Select(c => new {
                Course = c,
                Released = enrolledCourseIds.Contains(c.Id)
            })
            .OrderByDescending(x => x.Released)
            .ThenByDescending(x => x.Course.CreatedAt)
            .ToListAsync(cancellationToken);
            
        var result = courses.Select(x => new PortalMyCourseDto(
            x.Course.Id,
            x.Course.Name,
            x.Course.Description,
            x.Course.ImgCoverLink,
            x.Released,
            x.Course.CourseCategories.Select(cc => new Nexas.Application.Courses.Common.CourseCategoryBasicDto(cc.Category.Id, cc.Category.Name)).ToList()
        )).ToList();
        
        return result;
    }
}
