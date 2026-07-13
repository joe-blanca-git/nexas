namespace Nexas.Application.PortalHome.DTOs;

public record MyLatestLessonDto(
    int CourseId,
    string CourseTitle,
    int ModuleId,
    string ModuleTitle,
    int LessonId,
    string LessonTitle,
    int? LessonOrder,
    string? Thumbnail,
    DateTime LastViewedAt,
    int ProgressPercentage
);
