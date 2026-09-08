using MediatR;
using Nexas.Application.Common.Models;
using Nexas.Application.Tickets.DTOs;
using Nexas.Domain.Enums;
using System;

namespace Nexas.Application.Tickets.Queries.GetAdminTickets;

public record GetAdminTicketsQuery(TicketStatus? Status, int? CategoryId, TicketPriority? Priority, int? StudentId, string? SearchText, DateTime? StartDate, DateTime? EndDate, bool? NoReplyOnly, int PageIndex = 1, int PageSize = 10) : IRequest<PaginatedList<TicketAdminSummaryDto>>;
