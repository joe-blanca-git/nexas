using Nexas.Domain.Entities;

namespace Nexas.Application.Common.Interfaces;

/// <summary>
/// Contrato para integração com o gateway de pagamentos Asaas.
/// </summary>
public interface IAsaasService
{

    /// <summary>
    /// Cria um novo cliente no Asaas com base nos dados do usuário Nexas.
    /// </summary>
    Task<string> CreateCustomerAsync(User user, CancellationToken cancellationToken);

    /// <summary>
    /// Cria uma nova cobrança (compra avulsa) no Asaas.
    /// </summary>
    Task<string> CreatePaymentAsync(Purchase purchase, CancellationToken cancellationToken);

    /// <summary>
    /// Cria uma assinatura recorrente no Asaas.
    /// </summary>
    Task<string> CreateSubscriptionAsync(Subscription subscription, decimal amount, CancellationToken cancellationToken);

    /// <summary>
    /// Verifica o status de um pagamento específico.
    /// </summary>
    Task<string> GetPaymentStatusAsync(string asaasPaymentId, CancellationToken cancellationToken);
}