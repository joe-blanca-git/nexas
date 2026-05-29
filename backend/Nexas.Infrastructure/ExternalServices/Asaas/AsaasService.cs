using Microsoft.Extensions.Configuration;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.Purchases.Commands;
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
        _httpClient.DefaultRequestHeaders.Add("access_token", _apiKey);
    }

    public async Task<string> CreateCustomerAsync(User user, CancellationToken cancellationToken)
    {

        var requestData = new
        {
            name = user.Email, 
            email = user.Email,
            externalReference = user.Id.ToString()
        };

        var response = await _httpClient.PostAsJsonAsync("v3/customers", requestData, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AsaasResponse>(cancellationToken);
        return result?.Id ?? throw new Exception("Falha ao obter ID do cliente no Asaas.");
    }

    // NOTE: CreatePaymentAsync now accepts optional card info and returns a PurchaseResponseDto

    public async Task<string> CreateSubscriptionAsync(Subscription subscription, decimal amount, CancellationToken cancellationToken)
    {
        var requestData = new { 
            customer = subscription.User.AsaasCustomerId,
            billingType = "CREDIT_CARD",
            value = amount,
            cycle = "MONTHLY",
            externalReference = subscription.Id.ToString()
        };

        var response = await _httpClient.PostAsJsonAsync("v3/subscriptions", requestData, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AsaasResponse>(cancellationToken);
        return result?.Id ?? throw new Exception("Falha ao obter ID de assinatura do Asaas.");
    }

    public async Task<string> GetPaymentStatusAsync(string asaasPaymentId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"v3/payments/{asaasPaymentId}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AsaasResponse>(cancellationToken);
        return result?.Status ?? "UNKNOWN";
    }

    public async Task<PurchaseResponseDto> CreatePaymentAsync(Purchase purchase, CreditCardInfo? card, CancellationToken ct)
    {
        var requestData = new {
            customer = purchase.User.AsaasCustomerId,
            billingType = purchase.PaymentMethod.ToString(), // PIX ou CREDIT_CARD
            value = purchase.Amount,
            externalReference = purchase.Id.ToString(),
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
                postalCode = "00000000", // Placeholder ou do User
                addressNumber = "0"
            } : null
        };

        var response = await _httpClient.PostAsJsonAsync("v3/payments", requestData, ct);
        response.EnsureSuccessStatusCode();
        
        var asaasData = await response.Content.ReadFromJsonAsync<AsaasPaymentResult>(ct);
        if (asaasData == null) throw new Exception("Falha ao obter dados de pagamento do Asaas.");

        // Se for PIX, busca o QR Code
        string? qrCode = null, copyPaste = null;
        if (purchase.PaymentMethod == "PIX") {
            var pixResp = await _httpClient.GetAsync($"v3/payments/{asaasData.Id}/pixQrCode", ct);
            var pixData = await pixResp.Content.ReadFromJsonAsync<AsaasPixResult>(ct);
            qrCode = pixData.EncodedImage;
            copyPaste = pixData.Payload;
        }

        return new PurchaseResponseDto(purchase.Id, asaasData.Status, qrCode, copyPaste, asaasData.Id);
    }

    // Record genérico para capturar Ids e Status da API
    private record AsaasResponse(string Id, string Status);

    // Tipos específicos para resultados de pagamentos
    private record AsaasPaymentResult(string Id, string Status);
    private record AsaasPixResult(string EncodedImage, string Payload);
}