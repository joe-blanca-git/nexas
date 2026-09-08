using System;
using System.Collections.Generic;
using Nexas.Application.Portal.Tickets.DTOs;

namespace Nexas.Application.Tickets.DTOs;

public class TicketAdminSummaryDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastReplyAt { get; set; }
}

public class TicketAdminDetailsDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    
    // We can reuse the Portal DTOs for Messages and Timelines since the structure is identical
    public List<TicketMessageDto> Messages { get; set; } = new();
    public List<TicketTimelineDto> Timeline { get; set; } = new();
}

public class TicketDashboardDto
{
    public int OpenTickets { get; set; }
    public int PendingTickets { get; set; }
    public int RepliedTickets { get; set; }
    public int ClosedTickets { get; set; }
    public int TicketsToday { get; set; }
    public double AverageReplyTimeHours { get; set; }
    public double AverageCloseTimeHours { get; set; }
}

public class TicketStatsDto
{
    public Dictionary<string, int> ByCategory { get; set; } = new();
    public Dictionary<string, int> ByStatus { get; set; } = new();
}
