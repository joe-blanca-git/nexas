using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Infrastructure.Services;

public class BunnyNetService : IBunnyNetService
{
    private readonly IConfiguration _configuration;

    public BunnyNetService(IConfiguration configuration)
    {
        _configuration = configuration;
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
