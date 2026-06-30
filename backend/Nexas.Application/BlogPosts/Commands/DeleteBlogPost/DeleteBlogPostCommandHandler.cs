using MediatR;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.BlogPosts.Commands.DeleteBlogPost;

public class DeleteBlogPostCommandHandler : IRequestHandler<DeleteBlogPostCommand, Unit>
{
    private readonly INexasDbContext _context;

    public DeleteBlogPostCommandHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteBlogPostCommand request, CancellationToken cancellationToken)
    {
        var blogPost = await _context.BlogPosts.FindAsync(new object[] { request.Id }, cancellationToken);

        if (blogPost == null)
        {
            throw new InvalidOperationException($"Post de blog com ID {request.Id} não encontrado.");
        }

        _context.BlogPosts.Remove(blogPost);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
