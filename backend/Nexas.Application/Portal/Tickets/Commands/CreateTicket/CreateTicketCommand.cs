using MediatR;

namespace Nexas.Application.Portal.Tickets.Commands.CreateTicket;

public record CreateTicketCommand(string Subject, int CategoryId, string Content) : IRequest<int>;
