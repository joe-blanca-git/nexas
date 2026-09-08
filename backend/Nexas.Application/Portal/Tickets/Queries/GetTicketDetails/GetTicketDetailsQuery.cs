using MediatR;
using Nexas.Application.Portal.Tickets.DTOs;

namespace Nexas.Application.Portal.Tickets.Queries.GetTicketDetails;

public record GetTicketDetailsQuery(int Id) : IRequest<TicketDetailsDto?>;
