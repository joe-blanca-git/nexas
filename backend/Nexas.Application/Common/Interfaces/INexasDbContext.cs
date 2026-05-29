using Microsoft.EntityFrameworkCore;
using Nexas.Domain.Entities;

namespace Nexas.Application.Common.Interfaces
{
    public interface INexasDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Course> Courses { get; }
        DbSet<Module> Modules { get; }
        DbSet<Lesson> Lessons { get; }
        DbSet<Enrollment> Enrollments { get; }
        DbSet<Subscription> Subscriptions { get; }
        DbSet<Purchase> Purchases { get; }
        DbSet<SubscriptionPayment> SubscriptionPayments { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
