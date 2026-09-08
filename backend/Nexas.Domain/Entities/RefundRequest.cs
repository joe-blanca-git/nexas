using Nexas.Domain.Common;
using Nexas.Domain.Enums;

namespace Nexas.Domain.Entities;

public class RefundRequest : BaseEntity
{
    public int PurchaseId { get; private set; }
    public RefundStatus Status { get; private set; }
    public string? Reason { get; private set; }
    public string? SupportTicketCode { get; private set; }
    public string RequestCode { get; private set; } = null!;

    public virtual Purchase Purchase { get; private set; } = null!;

    private RefundRequest() { } // EF Core requirement

    private RefundRequest(int purchaseId, string requestCode, string? reason, string? supportTicketCode)
    {
        PurchaseId = purchaseId;
        RequestCode = requestCode;
        Reason = reason;
        SupportTicketCode = supportTicketCode;
        Status = RefundStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public static RefundRequest Create(int purchaseId, string requestCode, string? reason = null, string? supportTicketCode = null)
    {
        if (purchaseId <= 0) throw new ArgumentException("PurchaseId inválido.");
        if (string.IsNullOrWhiteSpace(requestCode)) throw new ArgumentException("RequestCode inválido.");

        return new RefundRequest(purchaseId, requestCode, reason, supportTicketCode);
    }

    public void Approve()
    {
        Status = RefundStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        Status = RefundStatus.Rejected;
        Reason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Process()
    {
        Status = RefundStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsRefunded()
    {
        Status = RefundStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string errorReason)
    {
        Status = RefundStatus.Failed;
        Reason = errorReason;
        UpdatedAt = DateTime.UtcNow;
    }
}
