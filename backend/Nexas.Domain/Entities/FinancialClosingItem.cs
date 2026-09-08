using Nexas.Domain.Common;

namespace Nexas.Domain.Entities;

public class FinancialClosingItem : BaseEntity
{
    public int FinancialClosingId { get; private set; }
    public int PurchaseId { get; private set; }
    
    public decimal AppliedTeacherPercentage { get; private set; }
    public decimal GrossValue { get; private set; }
    public decimal BankFeeValue { get; private set; }
    public decimal NexasFeeValue { get; private set; }
    public decimal CalculatedValue { get; private set; }

    // Navigation
    public virtual FinancialClosing FinancialClosing { get; private set; } = null!;
    public virtual Purchase Purchase { get; private set; } = null!;

    private FinancialClosingItem() { }

    public FinancialClosingItem(int purchaseId, decimal appliedTeacherPercentage, decimal grossValue, 
        decimal bankFeeValue, decimal nexasFeeValue, decimal calculatedValue)
    {
        PurchaseId = purchaseId;
        AppliedTeacherPercentage = appliedTeacherPercentage;
        GrossValue = grossValue;
        BankFeeValue = bankFeeValue;
        NexasFeeValue = nexasFeeValue;
        CalculatedValue = calculatedValue;
        CreatedAt = DateTime.UtcNow;
    }
}
