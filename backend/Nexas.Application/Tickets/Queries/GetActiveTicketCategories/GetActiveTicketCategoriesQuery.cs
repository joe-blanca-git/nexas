using MediatR;
using System.Collections.Generic;
using Nexas.Application.Tickets.DTOs;
namespace Nexas.Application.Tickets.Queries.GetActiveTicketCategories;
public record GetActiveTicketCategoriesQuery() : IRequest<List<TicketCategoryDto>>;
