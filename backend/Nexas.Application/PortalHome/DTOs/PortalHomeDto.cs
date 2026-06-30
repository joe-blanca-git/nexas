namespace Nexas.Application.PortalHome.DTOs;

public record PortalHomeDto(
    LatestCourseDto? LatestCourse,
    List<LatestNewsDto> LatestNews
);
