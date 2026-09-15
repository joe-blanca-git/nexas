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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Custmize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
