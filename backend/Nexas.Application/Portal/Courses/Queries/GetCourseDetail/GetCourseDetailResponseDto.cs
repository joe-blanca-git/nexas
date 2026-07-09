namespace Nexas.Application.Portal.Courses.Queries.GetCourseDetail;

public class GetCourseDetailResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Subtitle { get; set; }
    public string? ImgCoverLink { get; set; }
    public string? Teacher { get; set; }
    public string? Level { get; set; }
    public string? Category { get; set; }

    // Progress
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public int ProgressPercentage { get; set; }
    
    // Last Viewed Lesson
    public LastViewedLessonDto? LastViewedLesson { get; set; }

    // Content Structure
    public List<CourseModuleDto> Modules { get; set; } = new List<CourseModuleDto>();
}

public class LastViewedLessonDto
{
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public int ModuleId { get; set; }
    public string ModuleTitle { get; set; } = string.Empty;
}

public class CourseModuleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    
    public List<CourseLessonDto> Lessons { get; set; } = new List<CourseLessonDto>();
}

public class CourseLessonDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Duration { get; set; }
    public string? VideoUrl { get; set; }
    public int Order { get; set; }
    public bool IsCompleted { get; set; }
}
