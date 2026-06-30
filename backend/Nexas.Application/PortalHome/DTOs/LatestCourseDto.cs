namespace Nexas.Application.PortalHome.DTOs;

public record LatestCourseDto(
    int Id,
    string Title,
    string? Description,
    string? HeaderImageUrl,
    decimal Rating,
    int VoteCount
);
