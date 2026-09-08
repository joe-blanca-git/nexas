using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexas.Domain.Entities;

namespace Nexas.Infrastructure.Persistence.Configurations;

public class TicketTimelineConfiguration : IEntityTypeConfiguration<TicketTimeline>
{
    public void Configure(EntityTypeBuilder<TicketTimeline> builder)
    {
        builder.ToTable("TicketTimelines");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Event).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.HasOne(e => e.Ticket)
            .WithMany(t => t.Timelines)
            .HasForeignKey(e => e.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
