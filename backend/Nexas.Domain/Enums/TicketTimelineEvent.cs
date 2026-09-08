namespace Nexas.Domain.Enums;

public enum TicketTimelineEvent
{
    Created,
    Replied,
    Closed,
    Reopened,
    CategoryChanged,
    PriorityChanged,
    EmailSent,
    EmailReceived,
    AttachmentUploaded,
    StatusChanged
}
