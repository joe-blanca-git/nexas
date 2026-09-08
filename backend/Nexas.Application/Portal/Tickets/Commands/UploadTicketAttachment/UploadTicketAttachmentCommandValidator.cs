using FluentValidation;

namespace Nexas.Application.Portal.Tickets.Commands.UploadTicketAttachment;

public class UploadTicketAttachmentCommandValidator : AbstractValidator<UploadTicketAttachmentCommand>
{
    public UploadTicketAttachmentCommandValidator()
    {
        RuleFor(v => v.TicketId).GreaterThan(0);
        RuleFor(v => v.MessageId).GreaterThan(0);
        RuleFor(v => v.FileName).NotEmpty();
        RuleFor(v => v.ContentType).NotEmpty();
        RuleFor(v => v.Content).NotEmpty();
    }
}
