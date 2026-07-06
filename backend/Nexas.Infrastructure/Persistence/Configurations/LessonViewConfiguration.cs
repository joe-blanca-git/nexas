using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexas.Domain.Entities;

namespace Nexas.Infrastructure.Persistence.Configurations;

public class LessonViewConfiguration : IEntityTypeConfiguration<LessonView>
{
    public void Configure(EntityTypeBuilder<LessonView> builder)
    {
        builder.ToTable("LessonViews");

        builder.HasKey(lv => lv.Id);
        builder.Property(lv => lv.Id).HasColumnName("LessonViewId");

        builder.Property(lv => lv.UserId).IsRequired();
        builder.Property(lv => lv.LessonId).IsRequired();
        builder.Property(lv => lv.CreatedAt).HasColumnName("CreatedAt").IsRequired();
        builder.Property(lv => lv.UpdatedAt).HasColumnName("UpdatedAt");

        builder.HasIndex(lv => new { lv.UserId, lv.LessonId }).IsUnique();

        builder.HasOne(lv => lv.User)
            .WithMany(u => u.LessonViews)
            .HasForeignKey(lv => lv.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(lv => lv.Lesson)
            .WithMany(l => l.LessonViews)
            .HasForeignKey(lv => lv.LessonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
