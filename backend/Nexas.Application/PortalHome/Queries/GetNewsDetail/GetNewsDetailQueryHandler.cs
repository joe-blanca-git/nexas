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
        var blogPost = await _context.BlogPosts
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (blogPost == null)
        {
            throw new InvalidOperationException($"Notícia com ID {request.Id} não encontrada.");
        }

        return new NewsDetailDto(
            blogPost.Id,
            blogPost.Title,
            blogPost.Subject,
            blogPost.Content,
            blogPost.Tags,
            blogPost.HeaderImageUrl,
            blogPost.CreatedAt,
            blogPost.AuthorId
        );
    }
}
