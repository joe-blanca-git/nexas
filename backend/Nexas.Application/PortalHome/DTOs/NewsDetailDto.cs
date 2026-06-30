namespace Nexas.Application.PortalHome.DTOs;

public record NewsDetailDto(
    int Id,
    string Title,
    string Subject,
    string Content,
    string? Tags,
    string? HeaderImageUrl,
    DateTime PublishDate,
    int AuthorId
);
