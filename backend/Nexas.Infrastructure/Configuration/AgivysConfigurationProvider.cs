using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nexas.Infrastructure.Configuration;

public class AgivysConfigurationProvider : ConfigurationProvider
{
    private readonly AgivysConfigurationSource _source;

    public AgivysConfigurationProvider(AgivysConfigurationSource source)
    {
        _source = source;
    }

    public override void Load()
    {
        if (string.IsNullOrWhiteSpace(_source.Email) || string.IsNullOrWhiteSpace(_source.Password))
        {
            // Faltam credenciais, não faz nada
            return;
        }

        try
        {
            LoadAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            // Opcional: logar a exceção de falha ao buscar secrets
            Console.WriteLine($"[AgivysConfiguration] Error loading from Agivys: {ex.Message}");
        }
    }

    private async Task LoadAsync()
    {
        using var client = new HttpClient();

        // 1. Login
        var loginPayload = new { email = _source.Email, password = _source.Password };
        var loginResponse = await client.PostAsJsonAsync(_source.LoginUrl, loginPayload);
        
        if (!loginResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"[AgivysConfiguration] Login falhou com status {loginResponse.StatusCode}");
            return;
        }

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = loginResult.GetProperty("token").GetString();

        if (string.IsNullOrEmpty(token))
        {
            return;
        }

        // 2. Fetch Integrations
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var integrationsResponse = await client.GetAsync(_source.IntegrationsUrl);
        
        if (!integrationsResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"[AgivysConfiguration] Busca de integrações falhou com status {integrationsResponse.StatusCode}");
            return;
        }

        var integrations = await integrationsResponse.Content.ReadFromJsonAsync<JsonElement>();

        // Agivys API now returns { "value": [ ... ], "Count": X } (OData format)
        if (integrations.ValueKind == JsonValueKind.Object && integrations.TryGetProperty("value", out var valueArray))
        {
            integrations = valueArray;
        }

        if (integrations.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        var configData = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var integration in integrations.EnumerateArray())
        {
            var type = integration.GetProperty("type").GetString();
            var prefix = GetPrefixForType(type);

            if (string.IsNullOrEmpty(prefix))
                continue;

            if (integration.TryGetProperty("parameters", out var parameters) && parameters.ValueKind == JsonValueKind.Array)
            {
                foreach (var param in parameters.EnumerateArray())
                {
                    var key = param.GetProperty("key").GetString();
                    var value = param.GetProperty("value").GetString();

                    if (!string.IsNullOrEmpty(key))
                    {
                        // Exemplo: "Asaas:ApiKey" = "valor"
                        configData[$"{prefix}:{key}"] = value;
                    }
                }
            }
        }

        Data = configData;
    }

    private string GetPrefixForType(string? type)
    {
        return type switch
        {
            "email" => "EmailSettings",
            "files" => "Cloudflare",
            "videos" => "BunnyNets",
            "financial" => "Asaas",
            _ => string.Empty
        };
    }
}
