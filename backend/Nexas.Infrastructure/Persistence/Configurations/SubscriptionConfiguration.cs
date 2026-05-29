using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexas.Domain.Entities;

namespace Nexas.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("SubscriptionId");

        builder.Property(s => s.UserId)
            .HasColumnName("UserId");

        builder.Property(s => s.Status)
            .HasColumnName("Status")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(s => s.AsaasSubscriptionId)
            .HasColumnName("AsaasSubscriptionId")
            .HasMaxLength(100);

        builder.Property(s => s.StartDate)
            .HasColumnName("StartDate")
            .IsRequired(false);

        builder.Property(s => s.EndDate)
            .HasColumnName("EndDate")
            .IsRequired(false);

        builder.Property(s => s.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Ignore(s => s.PlanName);
        builder.Ignore(s => s.IsActive);
        builder.Ignore(s => s.UpdatedAt);

        // Relacionamento 1:N (Um Usuário pode ter várias assinaturas/histórico)
        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}