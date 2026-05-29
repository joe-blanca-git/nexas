using Nexas.Domain.Common;
using Nexas.Domain.Enums;

namespace Nexas.Domain.Entities
{
    public class SubscriptionPayment : BaseEntity
    {
        public int SubscriptionId { get; private set; }
        public virtual Subscription Subscription { get; private set; } = null!;

        public decimal Amount { get; private set; }
        public DateTime? BillingDate { get; private set; }
        public SubscriptionPaymentStatus Status { get; private set; }
        public string? AsaasPaymentId { get; private set; }

        private SubscriptionPayment()
        {
            // Required by EF Core
        }

        public static SubscriptionPayment Create(
            int subscriptionId,
            decimal amount,
            DateTime? billingDate,
            SubscriptionPaymentStatus status,
            string? asaasPaymentId = null)
        {
            if (subscriptionId <= 0)
                throw new ArgumentException("SubscriptionId must be greater than zero.", nameof(subscriptionId));

            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative.", nameof(amount));

            return new SubscriptionPayment
            {
                SubscriptionId = subscriptionId,
                Amount = amount,
                BillingDate = billingDate,
                Status = status,
                AsaasPaymentId = asaasPaymentId,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void UpdateStatus(SubscriptionPaymentStatus status)
        {
            Status = status;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAsaasPaymentId(string asaasPaymentId)
        {
            AsaasPaymentId = asaasPaymentId;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
