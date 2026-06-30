using MediatR;

namespace Nexas.Application.BlogPosts.Commands.DeleteBlogPost;

public record DeleteBlogPostCommand(int Id) : IRequest<Unit>;
