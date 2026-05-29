using Microsoft.Extensions.Configuration;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.Purchases.Commands;
using Nexas.Application.Subscriptions.Commands;
using Nexas.Domain.Entities;
using System.Net.Http.Json;

namespace Nexas.Infrastructure.ExternalServices.Asaas;

public class AsaasService : IAsaasService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public AsaasService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Asaas:ApiKey"] ?? throw new ArgumentNullException("Asaas ApiKey não configurada.");
        
        // Configura o token de acesso globalmente para todas as requisições deste serviço
        _httpClient.DefaultRequestHeaders.Add("access_token", _apiKey);
    }

    /// <summary>
    /// Cria um novo cliente no Asaas vinculado ao usuário do sistema.
    /// </summary>
    public async Task<string> CreateCustomerAsync(User user, CancellationToken cancellationToken)
    {
        var requestData = new
        {
            name = user.FullName ?? user.Email, 
            email = user.Email,
            cpfCnpj = user.CpfCnpj,
            externalReference = user.Id.ToString()
        };

        var response = await _httpClient.PostAsJsonAsync("v3/customers", requestData, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AsaasResponse>(cancellationToken);
        return result?.Id ?? throw new Exception("Falha ao obter ID do cliente no Asaas.");
    }

    /// <summary>
    /// Gera uma cobrança única (Curso) via PIX ou Cartão de Crédito.
    /// </summary>
    public async Task<PurchaseResponseDto> CreatePaymentAsync(Purchase purchase, CreditCardInfo? card, CancellationToken ct)
    {
        var requestData = new {
            customer = purchase.User.AsaasCustomerId,
            billingType = purchase.PaymentMethod == "PIX" ? "PIX" : "CREDIT_CARD",
            value = purchase.Amount,
            externalReference = purchase.Id.ToString(),
            // Dados do cartão se fornecidos (Checkout Transparente)
            creditCard = card != null ? new {
                holderName = card.HolderName,
                number = card.Number,
                expiryMonth = card.ExpiryMonth,
                expiryYear = card.ExpiryYear,
                ccv = card.Ccv
            } : null,
            creditCardHolderInfo = card != null ? new {
                name = card.HolderName,
                email = purchase.User.Email,
                cpfCnpj = card.HolderCpfCnpj,
                postalCode = "00000000", // Padrão se não coletado
                addressNumber = "0"
            } : null
        };

        var response = await _httpClient.PostAsJsonAsync("v3/payments", requestData, ct);
        response.EnsureSuccessStatusCode();
        
        var asaasData = await response.Content.ReadFromJsonAsync<AsaasPaymentResult>(ct)
            ?? throw new Exception("Falha ao obter dados de pagamento do Asaas.");

        // Se for PIX, busca os dados de QR Code e Copia e Cola
        string? qrCode = null, copyPaste = null;
        if (purchase.PaymentMethod == "PIX") {
            var pixResp = await _httpClient.GetAsync($"v3/payments/{asaasData.Id}/pixQrCode", ct);
            pixResp.EnsureSuccessStatusCode();
            var pixData = await pixResp.Content.ReadFromJsonAsync<AsaasPixResult>(ct)
                ?? throw new Exception("Falha ao obter dados PIX do pagamento Asaas.");
            qrCode = pixData.EncodedImage;
            copyPaste = pixData.Payload;
        }

        return new PurchaseResponseDto(purchase.Id, asaasData.Status, qrCode, copyPaste, asaasData.Id);
    }

    /// <summary>
    /// Cria uma assinatura recorrente mensal.
    /// </summary>
    public async Task<SubscriptionResponseDto> CreateSubscriptionAsync(
        Subscription subscription, 
        decimal amount, 
        CreditCardInfo? card, 
        CancellationToken ct)
    {
        var requestData = new
        {
            customer = subscription.User.AsaasCustomerId,
            billingType = card != null ? "CREDIT_CARD" : "PIX",
            value = amount,
            nextDueDate = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"),
            cycle = "MONTHLY",
            externalReference = subscription.Id.ToString(),
            description = "Assinatura Mensal Nexas",
            creditCard = card != null ? new {
                holderName = card.HolderName,
                number = card.Number,
                expiryMonth = card.ExpiryMonth,
                expiryYear = card.ExpiryYear,
                ccv = card.Ccv
            } : null,
            creditCardHolderInfo = card != null ? new {
                name = card.HolderName,
                email = subscription.User.Email,
                cpfCnpj = card.HolderCpfCnpj,
                postalCode = "00000000",
                addressNumber = "0"
            } : null
        };

        var response = await _httpClient.PostAsJsonAsync("v3/subscriptions", requestData, ct);
        response.EnsureSuccessStatusCode();

        var asaasData = await response.Content.ReadFromJsonAsync<AsaasSubscriptionResponse>(ct);

        return new SubscriptionResponseDto(
            subscription.Id, 
            asaasData?.Status ?? "pending", 
            asaasData?.Id);
    }

    public async Task<string> GetPaymentStatusAsync(string asaasPaymentId, CancellationToken ct)
    {
        var response = await _httpClient.GetAsync($"v3/payments/{asaasPaymentId}", ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AsaasPaymentResult>(ct);
        return result?.Status ?? "UNKNOWN";
    }

    // Mapeamentos internos das respostas do Gateway
    private record AsaasResponse(string Id);
    private record AsaasPaymentResult(string Id, string Status);
    private record AsaasPixResult(string EncodedImage, string Payload);
    private record AsaasSubscriptionResponse(string Id, string Status);
}