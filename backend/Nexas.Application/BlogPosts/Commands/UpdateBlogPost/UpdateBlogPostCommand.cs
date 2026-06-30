using MediatR;

namespace Nexas.Application.BlogPosts.Commands.UpdateBlogPost;

public record UpdateBlogPostCommand(
    int Id,
    string Title,
    string Subject,
    string Content,
    string? Tags,
    string? HeaderImageUrl
) : IRequest<Unit>;
