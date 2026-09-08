using MediatR;
using Nexas.Domain.Enums;
namespace Nexas.Application.Tickets.Commands.ChangeTicketStatus;
public record ChangeTicketStatusCommand(int TicketId, TicketStatus Status) : IRequest<bool>;
