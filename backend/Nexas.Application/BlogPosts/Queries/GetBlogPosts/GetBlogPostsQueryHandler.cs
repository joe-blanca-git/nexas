using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.BlogPosts.DTOs;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.BlogPosts.Queries.GetBlogPosts;

public class GetBlogPostsQueryHandler : IRequestHandler<GetBlogPostsQuery, List<BlogPostDto>>
{
    private readonly INexasDbContext _context;

    public GetBlogPostsQueryHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<List<BlogPostDto>> Handle(GetBlogPostsQuery request, CancellationToken cancellationToken)
    {
        return await _context.BlogPosts
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BlogPostDto(
                b.Id,
                b.AuthorId,
                b.Title,
                b.Subject,
                b.Content,
                b.Tags,
                b.HeaderImageUrl,
                b.CreatedAt,
                b.UpdatedAt
            ))
            .ToListAsync(cancellationToken);
    }
}
