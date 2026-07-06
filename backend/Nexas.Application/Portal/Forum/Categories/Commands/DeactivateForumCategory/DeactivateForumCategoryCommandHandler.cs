using MediatR;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Portal.Forum.Categories.Commands.DeactivateForumCategory;

public class DeactivateForumCategoryCommandHandler : IRequestHandler<DeactivateForumCategoryCommand>
{
    private readonly INexasDbContext _context;

    public DeactivateForumCategoryCommandHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeactivateForumCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ForumCategories.FindAsync(new object[] { request.Id }, cancellationToken);

        if (category == null)
            throw new Exception("Categoria não encontrada.");

        category.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
