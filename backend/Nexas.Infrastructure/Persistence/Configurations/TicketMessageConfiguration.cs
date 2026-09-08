using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexas.Domain.Entities;

namespace Nexas.Infrastructure.Persistence.Configurations;

public class TicketMessageConfiguration : IEntityTypeConfiguration<TicketMessage>
{
    public void Configure(EntityTypeBuilder<TicketMessage> builder)
    {
        builder.ToTable("TicketMessages");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Origin).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Content).IsRequired(); // text / nvarchar(max)
        builder.Property(e => e.EmailMessageId).HasMaxLength(300);
        builder.Property(e => e.Read).IsRequired();

        builder.HasOne(e => e.Ticket)
            .WithMany(t => t.Messages)
            .HasForeignKey(e => e.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.TicketId);
    }
}
