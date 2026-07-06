using MediatR;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

namespace Nexas.Application.Portal.Forum.Categories.Commands.CreateForumCategory;

public class CreateForumCategoryCommandHandler : IRequestHandler<CreateForumCategoryCommand, int>
{
    private readonly INexasDbContext _context;

    public CreateForumCategoryCommandHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateForumCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = ForumCategory.Create(request.Name, request.Description);

        _context.ForumCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
