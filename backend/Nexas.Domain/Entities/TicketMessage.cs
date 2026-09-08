using System;
using System.Collections.Generic;
using Nexas.Domain.Common;
using Nexas.Domain.Enums;

namespace Nexas.Domain.Entities;

public class TicketMessage : BaseEntity
{
    public int TicketId { get; private set; }
    public int UserId { get; private set; }
    public TicketOrigin Origin { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public string? EmailMessageId { get; private set; }
    public bool Read { get; private set; }

    // Navigation
    public virtual Ticket Ticket { get; private set; } = null!;
    public virtual User User { get; private set; } = null!;
    public virtual ICollection<TicketAttachment> Attachments { get; private set; } = new List<TicketAttachment>();

    private TicketMessage() { } // EF Core

    public static TicketMessage Create(int ticketId, int userId, TicketOrigin origin, string content, string? emailMessageId)
    {
        return new TicketMessage
        {
            TicketId = ticketId,
            UserId = userId,
            Origin = origin,
            Content = content,
            EmailMessageId = emailMessageId,
            Read = false,
            CreatedAt = DateTime.UtcNow
        };
    }
}
