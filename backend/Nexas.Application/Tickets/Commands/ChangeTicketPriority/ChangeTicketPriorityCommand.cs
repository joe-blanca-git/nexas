using MediatR;
using Nexas.Domain.Enums;
namespace Nexas.Application.Tickets.Commands.ChangeTicketPriority;
public record ChangeTicketPriorityCommand(int TicketId, TicketPriority Priority) : IRequest<bool>;
