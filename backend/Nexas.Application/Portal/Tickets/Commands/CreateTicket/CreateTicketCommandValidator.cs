using FluentValidation;

namespace Nexas.Application.Portal.Tickets.Commands.CreateTicket;

public class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {
        RuleFor(v => v.Subject).NotEmpty().WithMessage("Assunto é obrigatório.");
        RuleFor(v => v.CategoryId).GreaterThan(0).WithMessage("Categoria é obrigatória.");
        RuleFor(v => v.Content).NotEmpty().WithMessage("Conteúdo é obrigatório.");
    }
}
