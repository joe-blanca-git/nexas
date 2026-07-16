using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Infrastructure.Services;

public class BunnyNetService : IBunnyNetService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public BunnyNetService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<string?> CreateVideoLibraryAsync(string name)
    {
        var accountApiKey = _configuration["BunnyNets:AccountApiKey"];
        
        if (string.IsNullOrWhiteSpace(accountApiKey))
        {
            throw new InvalidOperationException("BunnyNets:AccountApiKey is not configured in appsettings.");
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "videolibrary");
        request.Headers.Add("AccessKey", accountApiKey);
        
        var payload = new { Name = name };
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create Bunny.net Library. Status: {response.StatusCode}, Body: {errorBody}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseBody);
        
        if (document.RootElement.TryGetProperty("Id", out var idElement))
        {
            return idElement.GetInt64().ToString();
        }

        return null;
    }

    private async Task<string> GetLibraryApiKeyAsync(string libraryId)
    {
        var accountApiKey = _configuration["BunnyNets:AccountApiKey"];
        
        if (string.IsNullOrWhiteSpace(accountApiKey))
            throw new InvalidOperationException("BunnyNets:AccountApiKey is not configured in appsettings.");

        var request = new HttpRequestMessage(HttpMethod.Get, $"videolibrary/{libraryId}");
        request.Headers.Add("AccessKey", accountApiKey);
        
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to fetch Library details from Bunny.net. Status: {response.StatusCode}, Body: {errorBody}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseBody);
        
        if (document.RootElement.TryGetProperty("ApiKey", out var keyElement))
        {
            return keyElement.GetString() ?? throw new Exception("Library ApiKey is null in Bunny.net response.");
        }
        
        throw new Exception("Could not find ApiKey in Bunny.net Library response.");
    }

    public async Task<string?> CreateCollectionAsync(string libraryId, string name)
    {
        // Dynamically fetch the API Key for this specific library
        var libraryApiKey = await GetLibraryApiKeyAsync(libraryId);

        // The Stream API uses video.bunnycdn.com
        var requestUrl = $"https://video.bunnycdn.com/library/{libraryId}/collections";
        var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        request.Headers.Add("AccessKey", libraryApiKey);
        
        var payload = new { name = name };
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create Bunny.net Collection. Status: {response.StatusCode}, Body: {errorBody}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseBody);
        
        if (document.RootElement.TryGetProperty("guid", out var guidElement))
        {
            return guidElement.GetString();
        }

        return null;
    }

    public string? GenerateSignedVideoUrl(string? libraryId, string? videoId, int expirationMinutes = 180)
    {
        if (string.IsNullOrWhiteSpace(libraryId) || string.IsNullOrWhiteSpace(videoId))
        {
            return null;
        }

        var securityKey = _configuration["BunnyNets:ApiKey"];
        
        if (string.IsNullOrWhiteSpace(securityKey))
        {
            // If there's no key configured, fallback to basic URL without token (though it might be rejected by Bunny)
            return $"https://iframe.mediadelivery.net/embed/{libraryId}/{videoId}?autoplay=false";
        }

        // 1. Calculate expiration time (Unix Timestamp)
        var expires = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes).ToUnixTimeSeconds();

        // 2. Prepare the string to be hashed
        var hashSource = $"{securityKey}{videoId}{expires}";

        // 3. Compute SHA256 Hash
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hashSource));
        var token = string.Concat(hashBytes.Select(b => b.ToString("x2")));

        // 4. Return the signed URL
        return $"https://iframe.mediadelivery.net/embed/{libraryId}/{videoId}?token={token}&expires={expires}&autoplay=false";
    }
}
