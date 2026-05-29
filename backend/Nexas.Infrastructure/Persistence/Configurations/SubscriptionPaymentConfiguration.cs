using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexas.Domain.Entities;

namespace Nexas.Infrastructure.Persistence.Configurations;

public class SubscriptionPaymentConfiguration : IEntityTypeConfiguration<SubscriptionPayment>
{
    public void Configure(EntityTypeBuilder<SubscriptionPayment> builder)
    {
        builder.ToTable("SubscriptionPayments");

        builder.HasKey(sp => sp.Id);
        builder.Property(sp => sp.Id).HasColumnName("PaymentId");

        builder.Property(sp => sp.SubscriptionId)
            .HasColumnName("SubscriptionId");

        builder.Property(sp => sp.Amount)
            .HasColumnName("Amount")
            .HasPrecision(10, 2);

        builder.Property(sp => sp.Status)
            .HasColumnName("Status")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(sp => sp.BillingDate)
            .HasColumnName("BillingDate")
            .IsRequired(false);

        builder.Property(sp => sp.AsaasPaymentId)
            .HasColumnName("AsaasPaymentId")
            .HasMaxLength(100);

        builder.Ignore(sp => sp.CreatedAt);
        builder.Ignore(sp => sp.UpdatedAt);

        // Relacionamento N:1 (Vários pagamentos pertencem a uma assinatura)
        builder.HasOne(sp => sp.Subscription)
            .WithMany()
            .HasForeignKey(sp => sp.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}