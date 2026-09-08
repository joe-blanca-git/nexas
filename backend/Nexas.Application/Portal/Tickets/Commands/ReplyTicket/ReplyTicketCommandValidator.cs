using FluentValidation;

namespace Nexas.Application.Portal.Tickets.Commands.ReplyTicket;

public class ReplyTicketCommandValidator : AbstractValidator<ReplyTicketCommand>
{
    public ReplyTicketCommandValidator()
    {
        RuleFor(v => v.TicketId).GreaterThan(0);
        RuleFor(v => v.Content).NotEmpty().WithMessage("Conteúdo da resposta é obrigatório.");
    }
}
