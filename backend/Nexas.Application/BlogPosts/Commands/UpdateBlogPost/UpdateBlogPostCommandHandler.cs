using MediatR;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.BlogPosts.Commands.UpdateBlogPost;

public class UpdateBlogPostCommandHandler : IRequestHandler<UpdateBlogPostCommand, Unit>
{
    private readonly INexasDbContext _context;

    public UpdateBlogPostCommandHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateBlogPostCommand request, CancellationToken cancellationToken)
    {
        var blogPost = await _context.BlogPosts.FindAsync(new object[] { request.Id }, cancellationToken);

        if (blogPost == null)
        {
            throw new InvalidOperationException($"Post de blog com ID {request.Id} não encontrado.");
        }

        blogPost.Update(
            request.Title,
            request.Subject,
            request.Content,
            request.Tags,
            request.HeaderImageUrl
        );

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
