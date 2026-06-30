namespace Nexas.Application.PortalHome.DTOs;

public record LatestNewsDto(
    int Id,
    string? HeaderImageUrl,
    string? Tags,
    string Title,
    string Subject,
    DateTime PublishDate
);
