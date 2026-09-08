using MediatR;
using FluentValidation;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

namespace Nexas.Application.ForumCategories.Commands.CreateForumCategory;

public record CreateForumCategoryCommand(string Name, string? Description, string? Icon) : IRequest<int>;

public class CreateForumCategoryCommandValidator : AbstractValidator<CreateForumCategoryCommand>
{
    public CreateForumCategoryCommandValidator()
    {
        RuleFor(v => v.Name)
            .MaximumLength(150).WithMessage("Name must not exceed 150 characters.")
            .NotEmpty().WithMessage("Name is required.");
    }
}

public class CreateForumCategoryCommandHandler : IRequestHandler<CreateForumCategoryCommand, int>
{
    private readonly INexasDbContext _context;

    public CreateForumCategoryCommandHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateForumCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = ForumCategory.Create(request.Name, request.Description, request.Icon);

        _context.ForumCategories.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
