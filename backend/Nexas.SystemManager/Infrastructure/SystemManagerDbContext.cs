using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nexas.SystemManager.Domain.Entities;

namespace Nexas.SystemManager.Infrastructure
{
    public class SystemManagerDbContext : IdentityDbContext<NexasUser>
    {
        public SystemManagerDbContext(DbContextOptions<SystemManagerDbContext> options)
            : base(options)
        {
        }
        public DbSet<Nexas.SystemManager.Domain.Entities.SystemApp> Applications { get; set; }
        public DbSet<Nexas.SystemManager.Domain.Entities.UserLoginHistory> LoginHistories { get; set; }
        public DbSet<Nexas.SystemManager.Domain.Entities.AppEndUser> AppEndUsers { get; set; }
        public DbSet<Nexas.SystemManager.Domain.Entities.AppRole> AppRoles { get; set; }
        public DbSet<Nexas.SystemManager.Domain.Entities.AppEndUserRole> AppEndUserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Custmize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            builder.Entity<Nexas.SystemManager.Domain.Entities.AppEndUserRole>()
                .HasKey(x => new { x.AppEndUserId, x.AppRoleId });
        }
    }
}
