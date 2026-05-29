using Nexas.Domain.Common;
using Nexas.Domain.Enums;

namespace Nexas.Domain.Entities
{
    public class Subscription : BaseEntity
    {
        public int UserId { get; private set; }
        public virtual User User { get; private set; } = null!;

        public string PlanName { get; private set; } = string.Empty;
        public DateTime? StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public bool IsActive { get; private set; }
        public SubscriptionStatus Status { get; private set; }
        public string? AsaasSubscriptionId { get; private set; }

        public virtual ICollection<SubscriptionPayment> Payments { get; private set; } = new List<SubscriptionPayment>();

        private Subscription()
        {
            // Required by EF Core
        }

        public static Subscription Create(
            int userId,
            string planName,
            DateTime? startDate,
            DateTime? endDate,
            bool isActive,
            SubscriptionStatus status,
            string? asaasSubscriptionId = null)
        {
            if (userId <= 0)
                throw new ArgumentException("UserId must be greater than zero.", nameof(userId));

            if (string.IsNullOrWhiteSpace(planName))
                throw new ArgumentException("Plan name cannot be null or whitespace.", nameof(planName));

            return new Subscription
            {
                UserId = userId,
                PlanName = planName,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = isActive,
                Status = status,
                AsaasSubscriptionId = asaasSubscriptionId,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void SetUser(User user)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public void UpdateAsaasSubscriptionId(string asaasSubscriptionId)
        {
            if (string.IsNullOrWhiteSpace(asaasSubscriptionId))
                throw new ArgumentException("O ID da assinatura Asaas não pode ser vazio.");

            AsaasSubscriptionId = asaasSubscriptionId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            Status = SubscriptionStatus.Active;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate(SubscriptionStatus status = SubscriptionStatus.Canceled)
        {
            IsActive = false;
            Status = status;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetEndDate(DateTime? endDate)
        {
            EndDate = endDate;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
