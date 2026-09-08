using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;
using Nexas.Domain.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.Tickets.Commands.ChangeTicketCategory;

public class ChangeTicketCategoryCommandHandler : IRequestHandler<ChangeTicketCategoryCommand, bool>
{
    private readonly INexasDbContext _context;
    private readonly IUserContextService _userContextService;

    public ChangeTicketCategoryCommandHandler(INexasDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<bool> Handle(ChangeTicketCategoryCommand request, CancellationToken cancellationToken)
    {
        var admin = await _userContextService.GetCurrentUserAsync();
        var ticket = await _context.Tickets.Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);
        if (ticket == null) return false;
        
        var oldCategory = ticket.Category?.Description ?? "Sem Categoria";
        
        var newCategoryEntity = await _context.TicketCategories.FindAsync(request.CategoryId);
        if (newCategoryEntity == null) return false;

        ticket.UpdateCategory(request.CategoryId);
        _context.TicketTimelines.Add(TicketTimeline.Create(ticket.Id, admin.Id, TicketTimelineEvent.CategoryChanged, $"Categoria alterada de '{oldCategory}' para '{newCategoryEntity.Description}'."));

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
