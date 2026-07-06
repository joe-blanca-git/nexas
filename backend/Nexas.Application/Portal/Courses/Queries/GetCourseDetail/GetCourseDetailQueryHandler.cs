using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Portal.Courses.Queries.GetCourseDetail;

public class GetCourseDetailQueryHandler : IRequestHandler<GetCourseDetailQuery, GetCourseDetailResponseDto?>
{
    private readonly INexasDbContext _context;
    private readonly IUserContextService _userContextService;

    public GetCourseDetailQueryHandler(INexasDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<GetCourseDetailResponseDto?> Handle(GetCourseDetailQuery request, CancellationToken cancellationToken)
    {
        var currentUser = await _userContextService.GetCurrentUserAsync();

        // 1. Fetch the course with related entities
        var course = await _context.Courses
            .Include(c => c.Modules)
                .ThenInclude(m => m.Lessons)
            .Include(c => c.CourseTeachers)
                .ThenInclude(ct => ct.Teacher)
            .Include(c => c.CourseCategories)
                .ThenInclude(cc => cc.Category)
            .AsSplitQuery() // Optimization for multiple includes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CourseId && c.Active, cancellationToken);

        if (course == null) return null;

        // Extract lesson IDs to find LessonViews for this user
        var courseLessonIds = course.Modules
            .SelectMany(m => m.Lessons)
            .Where(l => l.Active)
            .Select(l => l.Id)
            .ToList();

        // 2. Fetch User Progress (LessonViews)
        var lessonViews = await _context.LessonViews
            .AsNoTracking()
            .Where(lv => lv.UserId == currentUser.Id && courseLessonIds.Contains(lv.LessonId))
            .ToListAsync(cancellationToken);

        var completedLessonIds = lessonViews.Select(lv => lv.LessonId).ToHashSet();

        // 3. Build DTOs
        var response = new GetCourseDetailResponseDto
        {
            Id = course.Id,
            Title = course.Name,
            Description = course.Description,
            Subtitle = course.DescriptionSub,
            ImgCoverLink = course.ImgCoverLink,
            Teacher = course.CourseTeachers.FirstOrDefault()?.Teacher?.Name,
            Level = course.Level,
            Category = course.CourseCategories.FirstOrDefault()?.Category?.Name,
            TotalLessons = courseLessonIds.Count,
            CompletedLessons = completedLessonIds.Count,
            ProgressPercentage = courseLessonIds.Count > 0 ? (completedLessonIds.Count * 100) / courseLessonIds.Count : 0
        };

        // 4. Map Modules and Lessons
        int moduleOrder = 1;
        int globalLessonOrder = 1;
        
        foreach (var module in course.Modules.Where(m => m.Active).OrderBy(m => m.Id)) // Default order by Id if no Order field exists
        {
            var moduleDto = new CourseModuleDto
            {
                Id = module.Id,
                Title = module.Name,
                Description = module.Description,
                Order = moduleOrder++
            };

            foreach (var lesson in module.Lessons.Where(l => l.Active).OrderBy(l => l.Id))
            {
                moduleDto.Lessons.Add(new CourseLessonDto
                {
                    Id = lesson.Id,
                    Title = lesson.Name,
                    Description = lesson.Description,
                    Duration = lesson.DurationSeconds.HasValue ? $"{lesson.DurationSeconds / 60}m {lesson.DurationSeconds % 60}s" : null,
                    IdBunny = lesson.BunnyVideoId,
                    Order = globalLessonOrder++,
                    IsCompleted = completedLessonIds.Contains(lesson.Id)
                });
            }

            response.Modules.Add(moduleDto);
        }

        // 5. Determine Last Viewed Lesson
        var lastView = lessonViews.OrderByDescending(lv => lv.UpdatedAt ?? lv.CreatedAt).FirstOrDefault();
        if (lastView != null)
        {
            var lastLesson = course.Modules.SelectMany(m => m.Lessons).FirstOrDefault(l => l.Id == lastView.LessonId);
            var lastModule = course.Modules.FirstOrDefault(m => m.Id == lastLesson?.ModuleId);

            if (lastLesson != null && lastModule != null)
            {
                response.LastViewedLesson = new LastViewedLessonDto
                {
                    LessonId = lastLesson.Id,
                    LessonTitle = lastLesson.Name,
                    ModuleId = lastModule.Id,
                    ModuleTitle = lastModule.Name
                };
            }
        }

        return response;
    }
}
