using MediatR;

namespace Nexas.Application.Portal.Tickets.Commands.ReplyTicket;

public record ReplyTicketCommand(int TicketId, string Content) : IRequest<int>;
