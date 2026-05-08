using Nexas.Domain.Common;

namespace Nexas.Domain.Entities
{
    public class User : BaseEntity
    {
        public string ExternalId { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        
        // Relations
        public virtual ICollection<Enrollment> Enrollments { get; private set; } = new List<Enrollment>();
        public virtual ICollection<Subscription> Subscriptions { get; private set; } = new List<Subscription>();

        public static User Create(string externalId, string email)
        {
            return new User { ExternalId = externalId, Email = email };
        }
    }
}
