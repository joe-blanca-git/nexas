using MediatR;
using Nexas.Application.BlogPosts.DTOs;

namespace Nexas.Application.BlogPosts.Queries.GetBlogPostById;

public record GetBlogPostByIdQuery(int Id) : IRequest<BlogPostDto>;
