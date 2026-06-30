namespace Nexas.Application.PortalHome.DTOs;

public record LatestCourseDto(
    int Id,
    string Title,
    string? Description,
    decimal Rating,
    int VoteCount
);
