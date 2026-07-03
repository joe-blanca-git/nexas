using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexas.Domain.Entities;

namespace Nexas.Infrastructure.Persistence.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("EnrollmentId");

        builder.Property(e => e.UserId)
            .HasColumnName("UserId");

        builder.Property(e => e.CourseId)
            .HasColumnName("CourseId");

        // Mapeamento do novo campo Origin (Enum para String no BD)
        builder.Property(e => e.Origin)
            .HasColumnName("Origin")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Active)
            .HasColumnName("Active")
            .HasDefaultValue(true);

        // Índice Único (Garante que um aluno não tenha duas matrículas no mesmo curso)
        builder.HasIndex(e => new { e.UserId, e.CourseId })
            .IsUnique()
            .HasDatabaseName("IdxUniqueUserIdCourseId");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Ignore(e => e.UpdatedAt);

        // Relacionamentos
        builder.HasOne(e => e.User)
            .WithMany(u => u.Enrollments)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}