using FluentValidation;

namespace Nexas.Application.Portal.Forum.Messages.Commands.ReplyForumTopic;

public class ReplyForumTopicCommandValidator : AbstractValidator<ReplyForumTopicCommand>
{
    public ReplyForumTopicCommandValidator()
    {
        RuleFor(v => v.TopicId)
            .GreaterThan(0).WithMessage("Id do tópico inválido.");

        RuleFor(v => v.Content)
            .NotEmpty().WithMessage("O conteúdo da mensagem é obrigatório.")
            .MinimumLength(5).WithMessage("A mensagem deve ter pelo menos 5 caracteres.");
    }
}
