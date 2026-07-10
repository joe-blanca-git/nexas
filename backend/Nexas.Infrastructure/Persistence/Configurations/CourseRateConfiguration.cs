using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexas.Domain.Entities;

namespace Nexas.Infrastructure.Persistence.Configurations
{
    public class CourseRateConfiguration : IEntityTypeConfiguration<CourseRate>
    {
        public void Configure(EntityTypeBuilder<CourseRate> builder)
        {
            builder.ToTable("CourseRates");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Rate)
                .IsRequired();

            builder.Property(x => x.RatedAt)
                .IsRequired();

            // Chaves estrangeiras
            builder.HasOne(x => x.Course)
                .WithMany(x => x.CourseRates)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany(x => x.CourseRates)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice único para garantir uma avaliação por usuário por curso
            builder.HasIndex(x => new { x.CourseId, x.UserId })
                .IsUnique();
        }
    }
}
