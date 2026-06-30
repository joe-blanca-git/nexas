using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.BlogPosts.DTOs;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.BlogPosts.Queries.GetBlogPostById;

public class GetBlogPostByIdQueryHandler : IRequestHandler<GetBlogPostByIdQuery, BlogPostDto>
{
    private readonly INexasDbContext _context;

    public GetBlogPostByIdQueryHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<BlogPostDto> Handle(GetBlogPostByIdQuery request, CancellationToken cancellationToken)
    {
        var blogPost = await _context.BlogPosts
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (blogPost == null)
        {
            throw new InvalidOperationException($"Post de blog com ID {request.Id} não encontrado.");
        }

        return new BlogPostDto(
            blogPost.Id,
            blogPost.AuthorId,
            blogPost.Title,
            blogPost.Subject,
            blogPost.Content,
            blogPost.Tags,
            blogPost.HeaderImageUrl,
            blogPost.CreatedAt,
            blogPost.UpdatedAt
        );
    }
}
