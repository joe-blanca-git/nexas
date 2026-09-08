using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexas.Domain.Entities;

namespace Nexas.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Subject).IsRequired().HasMaxLength(300);
        builder.Property(e => e.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Priority).IsRequired().HasConversion<string>().HasMaxLength(50);

        builder.HasOne(e => e.Category)
            .WithMany(c => c.Tickets)
            .HasForeignKey(e => e.TicketCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.TicketCategoryId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.LastReplyAt);
    }
}
