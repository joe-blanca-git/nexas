using MediatR;
namespace Nexas.Application.Tickets.Commands.ChangeTicketCategory;
public record ChangeTicketCategoryCommand(int TicketId, int CategoryId) : IRequest<bool>;
