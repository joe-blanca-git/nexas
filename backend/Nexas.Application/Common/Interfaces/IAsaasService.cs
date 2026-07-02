using Nexas.Application.Purchases.Commands;
using Nexas.Application.Subscriptions.Commands;
using Nexas.Domain.Entities;

namespace Nexas.Application.Common.Interfaces;

public interface IAsaasService
{
    Task<string> CreateCustomerAsync(User user, CancellationToken ct);
    Task UpdateCustomerAsync(User user, CancellationToken ct);
    
    Task<PurchaseResponseDto> CreatePaymentAsync(Purchase purchase, CreditCardInfo? card, CancellationToken ct);

    Task<SubscriptionResponseDto> CreateSubscriptionAsync(Subscription subscription, decimal amount, CreditCardInfo? card, CancellationToken ct, int trialDays = 1);

    Task<string> CreatePixPaymentAsync(string asaasCustomerId, decimal amount, string description, CancellationToken ct);
    Task<PixQrCodeResponseDto> GetPixQrCodeAsync(string asaasPaymentId, CancellationToken ct);

    Task RefundPaymentAsync(string asaasPaymentId, CancellationToken ct);

    Task CancelSubscriptionAsync(string asaasSubscriptionId, CancellationToken ct);

    Task<string> GetPaymentStatusAsync(string asaasPaymentId, CancellationToken ct);
}