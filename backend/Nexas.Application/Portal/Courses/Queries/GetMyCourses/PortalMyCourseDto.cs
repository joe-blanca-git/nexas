namespace Nexas.Application.Portal.Courses.Queries.GetMyCourses;

public record PortalMyCourseDto(
    int Id,
    string Title,
    string? Description,
    string? ImgCoverLink,
    bool Released,
    string Color,
    string Code,
    decimal Rating,
    int Progress,
    int CompletedLessons,
    int TotalLessons,
    List<Nexas.Application.Courses.Common.CourseCategoryBasicDto>? Categories = null
);
