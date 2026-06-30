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
        return await (from b in _context.BlogPosts
                      join u in _context.Users on b.AuthorId equals u.Id into userGroup
                      from u in userGroup.DefaultIfEmpty()
                      orderby b.CreatedAt descending
                      select new BlogPostDto(
                          b.Id,
                          b.AuthorId,
                          u != null ? u.FullName : null,
                          b.Title,
                          b.Subject,
                          b.Content,
                          b.Tags,
                          b.HeaderImageUrl,
                          b.CreatedAt,
                          b.UpdatedAt
                      ))
                      .AsNoTracking()
                      .ToListAsync(cancellationToken);
    }
}
