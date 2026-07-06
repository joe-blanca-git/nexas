using FluentValidation;

namespace Nexas.Application.Portal.Forum.Topics.Commands.CreateForumTopic;

public class CreateForumTopicCommandValidator : AbstractValidator<CreateForumTopicCommand>
{
    public CreateForumTopicCommandValidator()
    {
        RuleFor(v => v.CategoryId)
            .GreaterThan(0).WithMessage("Id da categoria inválido.");

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("O título do tópico é obrigatório.")
            .MaximumLength(255).WithMessage("O título não pode exceder 255 caracteres.");

        RuleFor(v => v.Content)
            .NotEmpty().WithMessage("O conteúdo do tópico é obrigatório.");
    }
}
