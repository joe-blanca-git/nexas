using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.PortalHome.DTOs;

namespace Nexas.Application.PortalHome.Queries.GetHome;

public class GetHomeQueryHandler : IRequestHandler<GetHomeQuery, PortalHomeDto>
{
    private readonly INexasDbContext _context;
    private readonly IUserContextService _userContextService;

    public GetHomeQueryHandler(INexasDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<PortalHomeDto> Handle(GetHomeQuery request, CancellationToken cancellationToken)
    {
        var user = await _userContextService.GetCurrentUserAsync();

        var lastCourse = await _context.Courses
            .AsNoTracking()
            .Include(c => c.CourseCategories)
                .ThenInclude(cc => cc.Category)
            .Where(c => c.Active)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        LatestCourseDto? latestCourseDto = null;
        if (lastCourse != null)
        {
            latestCourseDto = new LatestCourseDto(
                lastCourse.Id,
                lastCourse.Name,
                lastCourse.Description,
                lastCourse.ImgCoverLink, // HeaderImageUrl
                0m, // Rating
                0,  // VoteCount
                lastCourse.CourseCategories.Select(cc => new Nexas.Application.Courses.Common.CourseCategoryBasicDto(cc.Category.Id, cc.Category.Name)).ToList()
            );
        }

        var latestNews = await _context.BlogPosts
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
            .Take(7)
            .Select(b => new LatestNewsDto(
                b.Id,
                b.HeaderImageUrl,
                b.Tags,
                b.Title,
                b.Subject,
                b.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var latestLessonData = await _context.LessonViews
            .AsNoTracking()
            .Where(lv => lv.UserId == user.Id)
            .OrderByDescending(lv => lv.CreatedAt)
            .Select(lv => new
            {
                CourseId = lv.Lesson.Module.CourseId,
                CourseTitle = lv.Lesson.Module.Course.Name,
                ModuleId = lv.Lesson.ModuleId,
                ModuleTitle = lv.Lesson.Module.Name,
                LessonId = lv.LessonId,
                LessonTitle = lv.Lesson.Name,
                Thumbnail = lv.Lesson.Module.Course.ImgCoverLink,
                LastViewedAt = lv.CreatedAt,
                TotalLessons = _context.Lessons.Count(l => l.Module.CourseId == lv.Lesson.Module.CourseId && l.Active && l.Module.Active),
                ViewedLessons = _context.LessonViews.Where(v => v.UserId == user.Id && v.Lesson.Module.CourseId == lv.Lesson.Module.CourseId).Select(v => v.LessonId).Distinct().Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        MyLatestLessonDto? latestLessonDto = null;
        if (latestLessonData != null)
        {
            latestLessonDto = new MyLatestLessonDto(
                latestLessonData.CourseId,
                latestLessonData.CourseTitle,
                latestLessonData.ModuleId,
                latestLessonData.ModuleTitle,
                latestLessonData.LessonId,
                latestLessonData.LessonTitle,
                null,
                latestLessonData.Thumbnail,
                latestLessonData.LastViewedAt,
                latestLessonData.TotalLessons > 0 ? (int)Math.Round((double)latestLessonData.ViewedLessons / latestLessonData.TotalLessons * 100) : 0
            );
        }

        var summaryData = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.UserId == user.Id && e.Active && e.Course.Active)
            .Select(e => new
            {
                CourseId = e.CourseId,
                TotalLessons = _context.Lessons.Count(l => l.Module.CourseId == e.CourseId && l.Active && l.Module.Active),
                ViewedLessons = _context.LessonViews.Where(lv => lv.UserId == user.Id && lv.Lesson.Module.CourseId == e.CourseId).Select(lv => lv.LessonId).Distinct().Count()
            })
            .ToListAsync(cancellationToken);

        int totalCourses = summaryData.Count;
        int completedCourses = summaryData.Count(x => x.TotalLessons > 0 && x.ViewedLessons >= x.TotalLessons);
        int coursesInProgress = summaryData.Count(x => x.TotalLessons > 0 && x.ViewedLessons > 0 && x.ViewedLessons < x.TotalLessons);
        int sumOfAllLessons = summaryData.Sum(x => x.TotalLessons);
        int sumOfAllViewedLessons = summaryData.Sum(x => x.ViewedLessons);
        int overallProgress = sumOfAllLessons > 0 ? (int)Math.Round((double)sumOfAllViewedLessons / sumOfAllLessons * 100) : 0;

        var coursesSummaryDto = new MyCoursesSummaryDto(
            totalCourses,
            completedCourses,
            coursesInProgress,
            overallProgress
        );

        return new PortalHomeDto(latestCourseDto, latestNews, latestLessonDto, coursesSummaryDto);
    }
}
