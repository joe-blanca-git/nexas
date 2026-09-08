using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexas.Domain.Entities;

namespace Nexas.Infrastructure.Persistence.Configurations
{
    public class AppUserRoleConfiguration : IEntityTypeConfiguration<AppUserRole>
    {
        public void Configure(EntityTypeBuilder<AppUserRole> builder)
        {
            builder.ToTable("AspNetUserRoles");
            builder.HasKey(ur => new { ur.UserId, ur.RoleId });
            
            builder.Ignore(ur => ur.Id);
            builder.Ignore(ur => ur.CreatedAt);
            builder.Ignore(ur => ur.UpdatedAt);

            builder.Property(ur => ur.UserId)
                .HasColumnName("UserId")
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(ur => ur.RoleId)
                .HasColumnName("RoleId")
                .IsRequired()
                .HasMaxLength(255);
        }
    }
}
