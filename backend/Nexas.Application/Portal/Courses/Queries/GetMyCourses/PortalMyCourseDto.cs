namespace Nexas.Application.Portal.Courses.Queries.GetMyCourses;

public record PortalMyCourseDto(
    int Id,
    string Title,
    string? Description,
    string? ImgCoverLink,
    bool Released,
    List<Nexas.Application.Courses.Common.CourseCategoryBasicDto>? Categories = null
);
