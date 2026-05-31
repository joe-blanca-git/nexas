using Microsoft.Extensions.Configuration;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.Purchases.Commands;
using Nexas.Application.Subscriptions.Commands;
using Nexas.Domain.Entities;
using System.Linq;
using System.Net.Http.Json;
using System.Net.Mail;

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
        var name = !string.IsNullOrWhiteSpace(user.FullName)
            ? user.FullName!
            : user.ExternalId;

        var email = NormalizeEmail(user.Email);
        var cpfCnpj = SanitizeCpfCnpj(user.CpfCnpj);

        var requestData = new Dictionary<string, object?>
        {
            ["name"] = name,
            ["externalReference"] = user.Id.ToString()
        };

        if (!string.IsNullOrWhiteSpace(email))
        {
            requestData["email"] = email;
        }

        if (!string.IsNullOrWhiteSpace(cpfCnpj))
        {
            requestData["cpfCnpj"] = cpfCnpj;
        }

        if (!requestData.ContainsKey("email") && !requestData.ContainsKey("cpfCnpj"))
        {
            throw new InvalidOperationException("Usuário precisa ter email válido ou CPF/CNPJ válido para criar cliente no Asaas.");
        }

        var response = await _httpClient.PostAsJsonAsync("customers", requestData, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Asaas.CreateCustomerAsync failed ({(int)response.StatusCode}): {response.ReasonPhrase}. Response body: {responseBody}");
        }

        var result = await response.Content.ReadFromJsonAsync<AsaasResponse>(cancellationToken);
        return result?.Id ?? throw new Exception("Falha ao obter ID do cliente no Asaas.");
    }

    /// <summary>
    /// Gera uma cobrança única (Curso) via PIX ou Cartão de Crédito.
    /// </summary>
    public async Task<PurchaseResponseDto> CreatePaymentAsync(Purchase purchase, CreditCardInfo? card, CancellationToken ct)
    {
        if (purchase.User == null || string.IsNullOrWhiteSpace(purchase.User.AsaasCustomerId))
        {
            throw new InvalidOperationException("Purchase.User or Purchase.User.AsaasCustomerId is required before creating a payment.");
        }

        var creditCardHolderInfo = card != null ? new Dictionary<string, object?>
        {
            ["name"] = card.HolderName,
            ["email"] = NormalizeEmail(purchase.User.Email),
            ["cpfCnpj"] = SanitizeCpfCnpj(card.HolderCpfCnpj),
            ["postalCode"] = "00000000", // Padrão se não coletado
            ["addressNumber"] = "0"
        } : null;

        if (creditCardHolderInfo != null)
        {
            if (creditCardHolderInfo["email"] == null)
            {
                creditCardHolderInfo.Remove("email");
            }

            if (creditCardHolderInfo["cpfCnpj"] == null)
            {
                creditCardHolderInfo.Remove("cpfCnpj");
            }
        }

        var requestData = new {
            customer = purchase.User.AsaasCustomerId,
            billingType = purchase.PaymentMethod == "PIX" ? "PIX" : "CREDIT_CARD",
            value = purchase.Amount,
            dueDate = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"),
            externalReference = purchase.Id.ToString(),
            // Dados do cartão se fornecidos (Checkout Transparente)
            creditCard = card != null ? new {
                holderName = card.HolderName,
                number = card.Number,
                expiryMonth = card.ExpiryMonth,
                expiryYear = card.ExpiryYear,
                ccv = card.Ccv
            } : null,
            creditCardHolderInfo = creditCardHolderInfo
        };

        var response = await _httpClient.PostAsJsonAsync("payments", requestData, ct);
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(ct);
            var requestBody = System.Text.Json.JsonSerializer.Serialize(requestData);
            throw new InvalidOperationException($"Asaas.CreatePaymentAsync failed ({(int)response.StatusCode}): {response.ReasonPhrase}. Response body: {responseBody}. Request body: {requestBody}");
        }

        var asaasData = await response.Content.ReadFromJsonAsync<AsaasPaymentResult>(ct)
            ?? throw new Exception("Falha ao obter dados de pagamento do Asaas.");

        // Se for PIX, busca os dados de QR Code e Copia e Cola
        string? qrCode = null, copyPaste = null;
        if (purchase.PaymentMethod == "PIX") {
            var pixResp = await _httpClient.GetAsync($"payments/{asaasData.Id}/pixQrCode", ct);
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
        CancellationToken ct,
        int trialDays = 1)
    {
        var creditCardHolderInfo = card != null ? new Dictionary<string, object?>
        {
            ["name"] = card.HolderName,
            ["email"] = NormalizeEmail(subscription.User.Email),
            ["cpfCnpj"] = SanitizeCpfCnpj(card.HolderCpfCnpj),
            ["postalCode"] = "00000000",
            ["addressNumber"] = "0"
        } : null;

        if (creditCardHolderInfo != null)
        {
            if (creditCardHolderInfo["email"] == null)
            {
                creditCardHolderInfo.Remove("email");
            }

            if (creditCardHolderInfo["cpfCnpj"] == null)
            {
                creditCardHolderInfo.Remove("cpfCnpj");
            }
        }

            var requestData = new
        {
            customer = subscription.User.AsaasCustomerId,
            billingType = card != null ? "CREDIT_CARD" : "PIX",
            value = amount,
            nextDueDate = DateTime.UtcNow.AddDays(trialDays).ToString("yyyy-MM-dd"),
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
            creditCardHolderInfo = creditCardHolderInfo
        };

        var response = await _httpClient.PostAsJsonAsync("subscriptions", requestData, ct);
        response.EnsureSuccessStatusCode();

        var asaasData = await response.Content.ReadFromJsonAsync<AsaasSubscriptionResponse>(ct);

        return new SubscriptionResponseDto(
            subscription.Id, 
            asaasData?.Status ?? "pending", 
            asaasData?.Id);
    }

    public async Task RefundPaymentAsync(string asaasPaymentId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(asaasPaymentId)) throw new ArgumentException("asaasPaymentId is required");

        var response = await _httpClient.PostAsync($"payments/{asaasPaymentId}/refund", null, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Asaas.RefundPaymentAsync failed: {(int)response.StatusCode} - {response.ReasonPhrase}. Body: {body}");
        }
    }

    public async Task CancelSubscriptionAsync(string asaasSubscriptionId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(asaasSubscriptionId)) throw new ArgumentException("asaasSubscriptionId is required");

        var response = await _httpClient.DeleteAsync($"subscriptions/{asaasSubscriptionId}", ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Asaas.CancelSubscriptionAsync failed: {(int)response.StatusCode} - {response.ReasonPhrase}. Body: {body}");
        }
    }

    public async Task<string> GetPaymentStatusAsync(string asaasPaymentId, CancellationToken ct)
    {
        var response = await _httpClient.GetAsync($"payments/{asaasPaymentId}", ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AsaasPaymentResult>(ct);
        return result?.Status ?? "UNKNOWN";
    }

    private static string? NormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        try
        {
            var address = new MailAddress(email);
            return address.Address;
        }
        catch
        {
            return null;
        }
    }

    private static string? SanitizeCpfCnpj(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length switch
        {
            11 => digits,
            14 => digits,
            _ => null
        };
    }

    // Mapeamentos internos das respostas do Gateway
    private record AsaasResponse(string Id);
    private record AsaasPaymentResult(string Id, string Status);
    private record AsaasPixResult(string EncodedImage, string Payload);
    private record AsaasSubscriptionResponse(string Id, string Status);
}