using Nexas.Domain.Common;
using Nexas.Domain.Enums;

namespace Nexas.Domain.Entities;

public class FinancialClosing : BaseEntity
{
    public int TeacherId { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    
    public decimal GrossRevenue { get; private set; }
    public decimal BankFeesTotal { get; private set; }
    public decimal NexasFeesTotal { get; private set; }
    public decimal NetValue { get; private set; }
    public decimal TotalToReceive { get; private set; }

    public ClosingStatus Status { get; private set; }
    public DateTime? PaymentDate { get; private set; }
    public string? AsaasTransferId { get; private set; }
    public string? PaymentReceiptUrl { get; private set; }

    // Navigation
    public virtual Teacher Teacher { get; private set; } = null!;
    public virtual ICollection<FinancialClosingItem> Items { get; private set; } = new List<FinancialClosingItem>();

    private FinancialClosing() { }

    public FinancialClosing(int teacherId, DateTime periodStart, DateTime periodEnd, decimal grossRevenue, 
        decimal bankFeesTotal, decimal nexasFeesTotal, decimal totalToReceive)
    {
        TeacherId = teacherId;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        GrossRevenue = grossRevenue;
        BankFeesTotal = bankFeesTotal;
        NexasFeesTotal = nexasFeesTotal;
        NetValue = grossRevenue - bankFeesTotal;
        TotalToReceive = totalToReceive;
        Status = ClosingStatus.PendingPayment;
        CreatedAt = DateTime.UtcNow;
    }

    public void Pay(string asaasTransferId, string? paymentReceiptUrl)
    {
        if (Status == ClosingStatus.Paid) throw new InvalidOperationException("Fechamento já está pago.");

        AsaasTransferId = asaasTransferId;
        PaymentReceiptUrl = paymentReceiptUrl;
        PaymentDate = DateTime.UtcNow;
        Status = ClosingStatus.Paid;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddItem(FinancialClosingItem item)
    {
        Items.Add(item);
    }
}
