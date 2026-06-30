namespace Nexas.Application.BlogPosts.DTOs;

public record BlogPostDto(
    int Id,
    int AuthorId,
    string? AuthorName,
    string Title,
    string Subject,
    string Content,
    string? Tags,
    string? HeaderImageUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
