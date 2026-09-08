using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexas.Domain.Entities;

namespace Nexas.Infrastructure.Persistence.Configurations
{
    public class AppRoleConfiguration : IEntityTypeConfiguration<AppRole>
    {
        public void Configure(EntityTypeBuilder<AppRole> builder)
        {
            builder.ToTable("AspNetRoles");
            builder.HasKey(r => r.RoleId);
            
            builder.Ignore(r => r.Id);
            builder.Ignore(r => r.CreatedAt);
            builder.Ignore(r => r.UpdatedAt);

            builder.Property(r => r.RoleId)
                .HasColumnName("Id")
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(r => r.Name)
                .HasColumnName("Name")
                .HasMaxLength(256);

            builder.Property(r => r.NormalizedName)
                .HasColumnName("NormalizedName")
                .HasMaxLength(256);
        }
    }
}
