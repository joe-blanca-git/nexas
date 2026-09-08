using MediatR;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

namespace Nexas.Application.ForumCategories.Commands.DeleteForumCategory;

public record DeleteForumCategoryCommand(int Id) : IRequest;

public class DeleteForumCategoryCommandHandler : IRequestHandler<DeleteForumCategoryCommand>
{
    private readonly INexasDbContext _context;

    public DeleteForumCategoryCommandHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteForumCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.ForumCategories
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new KeyNotFoundException($"ForumCategory with ID {request.Id} not found.");
        }

        _context.ForumCategories.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
