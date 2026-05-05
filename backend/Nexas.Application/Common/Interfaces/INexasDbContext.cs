using Microsoft.EntityFrameworkCore;
using Nexas.Domain.Entities;

namespace Nexas.Application.Common.Interfaces
{
    public interface INexasDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Course> Courses { get; }
        DbSet<Enrollment> Enrollments { get; }
        DbSet<Subscription> Subscriptions { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
