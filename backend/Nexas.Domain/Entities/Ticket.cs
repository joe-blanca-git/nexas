using System;
using System.Collections.Generic;
using Nexas.Domain.Common;
using Nexas.Domain.Enums;

namespace Nexas.Domain.Entities;

public class Ticket : BaseEntity
{
    public int UserId { get; private set; }
    public int TicketCategoryId { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public TicketStatus Status { get; private set; }
    public TicketPriority Priority { get; private set; }
    public DateTime? LastReplyAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    // Navigation
    public virtual TicketCategory Category { get; private set; } = null!;
    public virtual User User { get; private set; } = null!;
    public virtual ICollection<TicketMessage> Messages { get; private set; } = new List<TicketMessage>();
    public virtual ICollection<TicketTimeline> Timelines { get; private set; } = new List<TicketTimeline>();

    private Ticket() { } // EF Core

    public static Ticket Create(int userId, int categoryId, string subject, TicketPriority priority)
    {
        return new Ticket
        {
            UserId = userId,
            TicketCategoryId = categoryId,
            Subject = subject,
            Status = TicketStatus.Open,
            Priority = priority,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateStatus(TicketStatus newStatus)
    {
        Status = newStatus;
        if (newStatus == TicketStatus.Closed)
            ClosedAt = DateTime.UtcNow;
        else
            ClosedAt = null;
        
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLastReply()
    {
        LastReplyAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCategory(int categoryId)
    {
        TicketCategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePriority(TicketPriority priority)
    {
        Priority = priority;
        UpdatedAt = DateTime.UtcNow;
    }
}
