using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Enums;

namespace Nexas.Application.Teachers.Queries.GetTeacherDashboard;

public class GetTeacherDashboardQueryHandler : IRequestHandler<GetTeacherDashboardQuery, TeacherDashboardDto>
{
    private readonly INexasDbContext _context;

    public GetTeacherDashboardQueryHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherDashboardDto> Handle(GetTeacherDashboardQuery request, CancellationToken cancellationToken)
    {
        var dto = new TeacherDashboardDto();
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1);
        var previousMonthStart = currentMonthStart.AddMonths(-1);
        var nextMonthStart = currentMonthStart.AddMonths(1);
        var todayStart = now.Date;
        var todayEnd = todayStart.AddDays(1);

        // Find the teacher if requested
        var teacher = await _context.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.IdAgivys == request.IdAgivys && t.Active, cancellationToken);

        IQueryable<int>? teacherCourseIdsQuery = null;

        // If user is NOT Admin and is a registered teacher, filter specifically for that teacher.
        // If user IS Admin, teacherCourseIdsQuery remains null to aggregate across ALL platform courses.
        if (!request.IsAdmin && teacher != null)
        {
            teacherCourseIdsQuery = _context.CourseTeachers
                .Where(ct => ct.TeacherId == teacher.Id)
                .Select(ct => ct.CourseId);
        }

        // Purchases
        var approvedPurchasesQuery = _context.Purchases
            .AsNoTracking()
            .Where(p => p.Status == PurchaseStatus.Approved);

        if (teacherCourseIdsQuery != null)
        {
            approvedPurchasesQuery = approvedPurchasesQuery.Where(p => teacherCourseIdsQuery.Contains(p.CourseId));
        }

        var approvedPurchases = await approvedPurchasesQuery.ToListAsync(cancellationToken);

        dto.TotalCoursesSold = approvedPurchases.Count;
        dto.TotalRevenue = approvedPurchases.Sum(p => p.Amount);
        
        // This Month vs Previous Month
        var salesCurrentMonth = approvedPurchases.Count(p => p.CreatedAt >= currentMonthStart && p.CreatedAt < nextMonthStart);
        var salesPreviousMonth = approvedPurchases.Count(p => p.CreatedAt >= previousMonthStart && p.CreatedAt < currentMonthStart);
        
        dto.SalesGrowthPercentage = salesPreviousMonth == 0 
            ? (salesCurrentMonth > 0 ? 100 : 0)
            : Math.Round((decimal)(salesCurrentMonth - salesPreviousMonth) / salesPreviousMonth * 100, 2);

        var revCurrentMonth = approvedPurchases.Where(p => p.CreatedAt >= currentMonthStart && p.CreatedAt < nextMonthStart).Sum(p => p.Amount);
        var revPreviousMonth = approvedPurchases.Where(p => p.CreatedAt >= previousMonthStart && p.CreatedAt < currentMonthStart).Sum(p => p.Amount);
        
        dto.CurrentMonthRevenue = revCurrentMonth;
        dto.RevenueGrowthPercentage = revPreviousMonth == 0 
            ? (revCurrentMonth > 0 ? 100 : 0) 
            : Math.Round((revCurrentMonth - revPreviousMonth) / revPreviousMonth * 100, 2);

        dto.TodayRevenue = approvedPurchases.Where(p => p.CreatedAt >= todayStart && p.CreatedAt < todayEnd).Sum(p => p.Amount);

        // Monthly Revenue Array
        for (int i = 1; i <= 12; i++)
        {
            var monthStart = new DateTime(now.Year, i, 1);
            var monthEnd = monthStart.AddMonths(1);
            dto.MonthlyRevenueCurrentYear[i - 1] = approvedPurchases
                .Where(p => p.CreatedAt >= monthStart && p.CreatedAt < monthEnd)
                .Sum(p => p.Amount);
        }

        // Enrollments
        var enrollmentsQuery = _context.Enrollments
            .AsNoTracking()
            .Where(e => e.Active);

        if (teacherCourseIdsQuery != null)
        {
            enrollmentsQuery = enrollmentsQuery.Where(e => teacherCourseIdsQuery.Contains(e.CourseId));
        }

        var enrollments = await enrollmentsQuery.ToListAsync(cancellationToken);

        dto.TotalActiveStudents = enrollments.Select(e => e.UserId).Distinct().Count();
        
        dto.NewStudentsThisMonth = enrollments
            .Where(e => e.CreatedAt >= currentMonthStart && e.CreatedAt < nextMonthStart)
            .Select(e => e.UserId)
            .Distinct()
            .Count();

        // Active published courses
        var coursesQuery = _context.Courses.AsNoTracking().Where(c => c.Active);
        if (teacherCourseIdsQuery != null)
        {
            coursesQuery = coursesQuery.Where(c => teacherCourseIdsQuery.Contains(c.Id));
        }
        dto.TotalActivePublishedCourses = await coursesQuery.CountAsync(cancellationToken);

        // Active classes
        var classesQuery = _context.Lessons.AsNoTracking().Where(l => l.Module.Course.Active);
        if (teacherCourseIdsQuery != null)
        {
            classesQuery = classesQuery.Where(l => teacherCourseIdsQuery.Contains(l.Module.CourseId));
        }
        dto.TotalPublishedClassesOfActiveCourses = await classesQuery.CountAsync(cancellationToken);

        // Ratings
        var ratingsQuery = _context.CourseRates.AsNoTracking().AsQueryable();
        if (teacherCourseIdsQuery != null)
        {
            ratingsQuery = ratingsQuery.Where(cr => teacherCourseIdsQuery.Contains(cr.CourseId));
        }
        var ratings = await ratingsQuery.ToListAsync(cancellationToken);
            
        dto.AverageCourseRating = ratings.Any() ? Math.Round((decimal)ratings.Average(r => r.Rate), 1) : 0;

        // Forums
        var teacherTopicsQuery = _context.ForumTopics
            .AsNoTracking()
            .Include(f => f.Author)
            .Include(f => f.Lesson)
                .ThenInclude(l => l.Module)
                    .ThenInclude(m => m.Course)
            .AsQueryable();

        if (teacherCourseIdsQuery != null)
        {
            teacherTopicsQuery = teacherTopicsQuery.Where(f => f.LessonId != null && f.Lesson != null && teacherCourseIdsQuery.Contains(f.Lesson.Module.CourseId));
        }

        dto.TotalOpenForumsWithoutReply = await teacherTopicsQuery
            .CountAsync(f => f.Status == ForumTopicStatus.Open && !f.Messages.Any(), cancellationToken);

        var lastForums = await teacherTopicsQuery
            .OrderByDescending(f => f.CreatedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        dto.Last5Forums = lastForums.Select(f => new DashboardForumDto
        {
            ForumId = f.CategoryId,
            TopicId = f.Id,
            CourseName = f.Lesson?.Module?.Course?.Name ?? "N/A",
            LessonName = f.Lesson?.Name ?? "N/A",
            Title = f.Title,
            AuthorName = f.Author?.FullName ?? "Desconhecido",
            Date = f.CreatedAt,
            Status = f.Status.ToString()
        }).ToList();

        return dto;
    }
}
