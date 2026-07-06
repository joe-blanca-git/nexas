using MediatR;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Portal.Forum.Categories.Commands.UpdateForumCategory;

public class UpdateForumCategoryCommandHandler : IRequestHandler<UpdateForumCategoryCommand>
{
    private readonly INexasDbContext _context;

    public UpdateForumCategoryCommandHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateForumCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ForumCategories.FindAsync(new object[] { request.Id }, cancellationToken);

        if (category == null)
            throw new Exception("Categoria não encontrada.");

        category.Update(request.Name, request.Description);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
