using System;
using System.Collections.Generic;
using Nexas.Domain.Enums;

namespace Nexas.Application.Portal.Tickets.DTOs;

public class TicketSummaryDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastReplyAt { get; set; }
}

public class TicketDetailsDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public List<TicketMessageDto> Messages { get; set; } = new();
    public List<TicketTimelineDto> Timeline { get; set; } = new();
}

public class TicketMessageDto
{
    public int Id { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<TicketAttachmentDto> Attachments { get; set; } = new();
}

public class TicketAttachmentDto
{
    public int Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty; // Temporary URL for download
}

public class TicketTimelineDto
{
    public int Id { get; set; }
    public string Event { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
