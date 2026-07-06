using FluentValidation;

namespace Nexas.Application.Portal.Forum.Categories.Commands.UpdateForumCategory;

public class UpdateForumCategoryCommandValidator : AbstractValidator<UpdateForumCategoryCommand>
{
    public UpdateForumCategoryCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0).WithMessage("Id da categoria inválido.");

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("O nome da categoria é obrigatório.")
            .MaximumLength(150).WithMessage("O nome da categoria não pode exceder 150 caracteres.");

        RuleFor(v => v.Description)
            .MaximumLength(500).WithMessage("A descrição não pode exceder 500 caracteres.");
    }
}
