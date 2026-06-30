using MediatR;
using Nexas.Application.BlogPosts.DTOs;

namespace Nexas.Application.BlogPosts.Queries.GetBlogPosts;

public record GetBlogPostsQuery : IRequest<List<BlogPostDto>>;
