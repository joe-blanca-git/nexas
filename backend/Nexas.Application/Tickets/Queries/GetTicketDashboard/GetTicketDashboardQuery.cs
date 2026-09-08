using MediatR;
using Nexas.Application.Tickets.DTOs;
namespace Nexas.Application.Tickets.Queries.GetTicketDashboard;
public record GetTicketDashboardQuery() : IRequest<TicketDashboardDto>;
