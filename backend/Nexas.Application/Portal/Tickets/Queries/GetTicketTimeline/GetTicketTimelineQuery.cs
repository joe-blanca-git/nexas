using MediatR;
using Nexas.Application.Portal.Tickets.DTOs;
using System.Collections.Generic;

namespace Nexas.Application.Portal.Tickets.Queries.GetTicketTimeline;

public record GetTicketTimelineQuery(int TicketId) : IRequest<List<TicketTimelineDto>?>;
