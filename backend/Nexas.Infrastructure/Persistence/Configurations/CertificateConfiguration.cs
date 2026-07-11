using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexas.Domain.Entities;

namespace Nexas.Infrastructure.Persistence.Configurations;

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.ToTable("Certificates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ValidationCode)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.IssuedAt)
            .IsRequired();

        // Relacionamento com User
        builder.HasOne(x => x.User)
            .WithMany(x => x.Certificates)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento com Course
        builder.HasOne(x => x.Course)
            .WithMany(x => x.Certificates)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índice único para ValidationCode
        builder.HasIndex(x => x.ValidationCode)
            .IsUnique();

        // Índice único composto (um usuário só pode ter um certificado por curso)
        builder.HasIndex(x => new { x.UserId, x.CourseId })
            .IsUnique();
    }
}
