using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.PortalHome.DTOs;

namespace Nexas.Application.PortalHome.Queries.GetNewsDetail;

public class GetNewsDetailQueryHandler : IRequestHandler<GetNewsDetailQuery, NewsDetailDto>
{
    private readonly INexasDbContext _context;

    public GetNewsDetailQueryHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<NewsDetailDto> Handle(GetNewsDetailQuery request, CancellationToken cancellationToken)
    {
        var result = await (from b in _context.BlogPosts
                            join u in _context.Users on b.AuthorId equals u.Id into userGroup
                            from u in userGroup.DefaultIfEmpty()
                            where b.Id == request.Id
                            select new
                            {
                                Post = b,
                                AuthorName = u != null ? u.FullName : null
                            })
                            .AsNoTracking()
                            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
        {
            throw new InvalidOperationException($"Notícia com ID {request.Id} não encontrada.");
        }

        var blogPost = result.Post;

        return new NewsDetailDto(
            blogPost.Id,
            blogPost.Title,
            blogPost.Subject,
            blogPost.Content,
            blogPost.Tags,
            blogPost.HeaderImageUrl,
            blogPost.CreatedAt,
            blogPost.AuthorId,
            result.AuthorName
        );
    }
}
