using MediatR;

namespace Nexas.Application.BlogPosts.Commands.CreateBlogPost;

public record CreateBlogPostCommand(
    string Title,
    string Subject,
    string Content,
    string? Tags,
    string? HeaderImageUrl
) : IRequest<int>;
