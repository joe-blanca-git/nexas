using MediatR;
using Nexas.Application.Tickets.DTOs;
namespace Nexas.Application.Tickets.Queries.GetAdminTicketDetails;
public record GetAdminTicketDetailsQuery(int Id) : IRequest<TicketAdminDetailsDto?>;
