using System;
using Nexas.Domain.Common;
using Nexas.Domain.Enums;

namespace Nexas.Domain.Entities;

public class TicketTimeline : BaseEntity
{
    public int TicketId { get; private set; }
    public int UserId { get; private set; }
    public TicketTimelineEvent Event { get; private set; }
    public string? Description { get; private set; }

    // Navigation
    public virtual Ticket Ticket { get; private set; } = null!;
    public virtual User User { get; private set; } = null!;

    private TicketTimeline() { } // EF Core

    public static TicketTimeline Create(int ticketId, int userId, TicketTimelineEvent ev, string? description)
    {
        return new TicketTimeline
        {
            TicketId = ticketId,
            UserId = userId,
            Event = ev,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }
}
