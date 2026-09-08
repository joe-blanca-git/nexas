using MediatR;
namespace Nexas.Application.Tickets.Commands.AdminReplyTicket;
public record AdminReplyTicketCommand(int TicketId, string Content) : IRequest<int>;
