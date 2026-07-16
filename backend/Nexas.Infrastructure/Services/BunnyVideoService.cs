using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.Common.Models.Bunny;

namespace Nexas.Infrastructure.Services
{
    public class BunnyVideoService : IBunnyVideoService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public BunnyVideoService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        private async Task<string> GetLibraryApiKeyAsync(int libraryId, CancellationToken cancellationToken)
        {
            var accountApiKey = _configuration["BunnyNets:AccountApiKey"];
            
            if (string.IsNullOrWhiteSpace(accountApiKey))
                throw new InvalidOperationException("BunnyNets:AccountApiKey is not configured in appsettings.");

            var requestUrl = $"https://api.bunny.net/videolibrary/{libraryId}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("AccessKey", accountApiKey);
            
            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new Exception($"Failed to fetch Library details from Bunny.net. Status: {response.StatusCode}, Body: {errorBody}");
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(responseBody);
            
            if (document.RootElement.TryGetProperty("ApiKey", out var keyElement))
            {
                return keyElement.GetString() ?? throw new Exception("Library ApiKey is null in Bunny.net response.");
            }
            
            throw new Exception("Could not find ApiKey in Bunny.net Library response.");
        }

        public async Task<VideoResponseDto> CreateVideoAsync(int libraryId, string collectionId, string title, CancellationToken cancellationToken)
        {
            var libraryApiKey = await GetLibraryApiKeyAsync(libraryId, cancellationToken);
            var requestUrl = $"https://video.bunnycdn.com/library/{libraryId}/videos";
            
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Add("AccessKey", libraryApiKey);
            
            var payload = new 
            { 
                title = title,
                collectionId = collectionId 
            };
            
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new Exception($"Failed to create video in Bunny.net. Status: {response.StatusCode}, Body: {errorBody}");
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(responseBody);
            
            var guid = document.RootElement.GetProperty("guid").GetString();
            
            return new VideoResponseDto 
            { 
                Guid = guid ?? "", 
                Title = title,
                Status = 0 // Assuming 0 is the starting status
            };
        }

        public async Task<VideoUploadInformationDto> GenerateUploadInformationAsync(int libraryId, string bunnyVideoId, CancellationToken cancellationToken)
        {
            var libraryApiKey = await GetLibraryApiKeyAsync(libraryId, cancellationToken);
            
            // Official Bunny Stream TUS Authentication Mechanism
            // AuthorizationSignature = SHA256(LibraryApiKey + VideoId + ExpirationTime)
            
            var expirationTime = DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds();
            var hashSource = $"{libraryApiKey}{bunnyVideoId}{expirationTime}";

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hashSource));
            var signature = string.Concat(hashBytes.Select(b => b.ToString("x2")));

            var dto = new VideoUploadInformationDto
            {
                BunnyVideoId = bunnyVideoId,
                UploadUrl = "https://video.bunnycdn.com/tusupload",
                Headers = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "AuthorizationSignature", signature },
                    { "AuthorizationExpire", expirationTime.ToString() },
                    { "VideoId", bunnyVideoId },
                    { "LibraryId", libraryId.ToString() }
                }
            };

            return dto;
        }

        public async Task<VideoResponseDto> GetVideoAsync(int libraryId, string bunnyVideoId, CancellationToken cancellationToken)
        {
            var libraryApiKey = await GetLibraryApiKeyAsync(libraryId, cancellationToken);
            var requestUrl = $"https://video.bunnycdn.com/library/{libraryId}/videos/{bunnyVideoId}";
            
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("AccessKey", libraryApiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new Exception($"Failed to get video from Bunny.net. Status: {response.StatusCode}, Body: {errorBody}");
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(responseBody);
            
            var guid = document.RootElement.GetProperty("guid").GetString() ?? "";
            var title = document.RootElement.GetProperty("title").GetString() ?? "";
            var status = document.RootElement.GetProperty("status").GetInt32();
            
            return new VideoResponseDto 
            { 
                Guid = guid, 
                Title = title,
                Status = status
            };
        }

        public async Task<int> GetVideoStatusAsync(int libraryId, string bunnyVideoId, CancellationToken cancellationToken)
        {
            var video = await GetVideoAsync(libraryId, bunnyVideoId, cancellationToken);
            return video.Status;
        }

        public async Task<bool> DeleteVideoAsync(int libraryId, string bunnyVideoId, CancellationToken cancellationToken)
        {
            var libraryApiKey = await GetLibraryApiKeyAsync(libraryId, cancellationToken);
            var requestUrl = $"https://video.bunnycdn.com/library/{libraryId}/videos/{bunnyVideoId}";
            
            var request = new HttpRequestMessage(HttpMethod.Delete, requestUrl);
            request.Headers.Add("AccessKey", libraryApiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
    }
}
