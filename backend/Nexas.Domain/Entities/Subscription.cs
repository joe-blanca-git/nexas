using Nexas.Domain.Common;

namespace Nexas.Domain.Entities
{
    public class Subscription : BaseEntity
    {
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        public string PlanName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
