using System;
using System.Collections.Generic;
using Nexas.Domain.Common;

namespace Nexas.Domain.Entities;

public class TicketCategory : BaseEntity
{
    public string Description { get; private set; } = string.Empty;
    public string? Icon { get; private set; }
    public bool Active { get; private set; } = true;

    // Navigation
    public virtual ICollection<Ticket> Tickets { get; private set; } = new List<Ticket>();

    private TicketCategory() { } // EF Core

    public static TicketCategory Create(string description, string? icon)
    {
        return new TicketCategory
        {
            Description = description,
            Icon = icon,
            Active = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
