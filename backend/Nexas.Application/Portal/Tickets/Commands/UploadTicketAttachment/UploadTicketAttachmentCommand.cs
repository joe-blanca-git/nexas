using MediatR;

namespace Nexas.Application.Portal.Tickets.Commands.UploadTicketAttachment;

public record UploadTicketAttachmentCommand(int TicketId, int MessageId, string FileName, string ContentType, byte[] Content) : IRequest<int>;
