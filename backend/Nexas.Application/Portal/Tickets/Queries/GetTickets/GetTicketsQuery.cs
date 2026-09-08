using MediatR;
using Nexas.Application.Common.Models;
using Nexas.Application.Portal.Tickets.DTOs;
using Nexas.Domain.Enums;

namespace Nexas.Application.Portal.Tickets.Queries.GetTickets;

public record GetTicketsQuery(TicketStatus? Status, int? CategoryId, string? SearchText, int PageIndex = 1, int PageSize = 10) : IRequest<PaginatedList<TicketSummaryDto>>;
