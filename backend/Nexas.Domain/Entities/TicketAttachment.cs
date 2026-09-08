using System;
using Nexas.Domain.Common;

namespace Nexas.Domain.Entities;

public class TicketAttachment : BaseEntity
{
    public int TicketMessageId { get; private set; }
    public string OriginalFileName { get; private set; } = string.Empty;
    public string StoredFileName { get; private set; } = string.Empty;
    public string Bucket { get; private set; } = string.Empty;
    public string Path { get; private set; } = string.Empty;
    public string MimeType { get; private set; } = string.Empty;
    public long Size { get; private set; }

    // Navigation
    public virtual TicketMessage Message { get; private set; } = null!;

    private TicketAttachment() { } // EF Core

    public static TicketAttachment Create(int messageId, string originalFileName, string storedFileName, string bucket, string path, string mimeType, long size)
    {
        return new TicketAttachment
        {
            TicketMessageId = messageId,
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            Bucket = bucket,
            Path = path,
            MimeType = mimeType,
            Size = size,
            CreatedAt = DateTime.UtcNow
        };
    }
}
