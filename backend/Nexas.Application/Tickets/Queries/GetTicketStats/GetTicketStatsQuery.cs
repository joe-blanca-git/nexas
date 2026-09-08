using MediatR;
using Nexas.Application.Tickets.DTOs;
namespace Nexas.Application.Tickets.Queries.GetTicketStats;
public record GetTicketStatsQuery() : IRequest<TicketStatsDto>;
