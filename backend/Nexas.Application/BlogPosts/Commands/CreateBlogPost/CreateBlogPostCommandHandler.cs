using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

namespace Nexas.Application.BlogPosts.Commands.CreateBlogPost;

public class CreateBlogPostCommandHandler : IRequestHandler<CreateBlogPostCommand, int>
{
    private readonly INexasDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateBlogPostCommandHandler(INexasDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<int> Handle(CreateBlogPostCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.ExternalId == _currentUserService.ExternalId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("Usuário não encontrado.");
        }

        var blogPost = BlogPost.Create(
            user.Id,
            request.Title,
            request.Subject,
            request.Content,
            request.Tags,
            request.HeaderImageUrl
        );

        _context.BlogPosts.Add(blogPost);
        await _context.SaveChangesAsync(cancellationToken);

        return blogPost.Id;
    }
}
