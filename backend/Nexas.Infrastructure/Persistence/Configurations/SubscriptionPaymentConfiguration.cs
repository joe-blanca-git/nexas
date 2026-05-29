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

        builder.Property(sp => sp.Amount)
            .HasPrecision(10, 2);

        builder.Property(sp => sp.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(sp => sp.BillingDate)
            .IsRequired(false);

        builder.Property(sp => sp.AsaasPaymentId)
            .HasMaxLength(100);

        // Relacionamento N:1 (Vários pagamentos pertencem a uma assinatura)
        builder.HasOne(sp => sp.Subscription)
            .WithMany()
            .HasForeignKey(sp => sp.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}