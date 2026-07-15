using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Infrastructure.Services;

public class CloudflareStorageService : ICloudflareStorageService
{
    private readonly IConfiguration _configuration;
    private readonly AmazonS3Client _s3Client;
    private readonly string _bucketName = "nexas";
    private readonly string _publicUrl;

    public CloudflareStorageService(IConfiguration configuration)
    {
        _configuration = configuration;
        var accessKey = _configuration["Cloudflare:AccessKeyId"];
        var secretKey = _configuration["Cloudflare:SecretTokenKey"] ?? _configuration["Cloudflare:TokenKey"];
        var serviceUrl = _configuration["Cloudflare:UrlS3Client"];
        _publicUrl = _configuration["Cloudflare:UrlBuckS3Api"];

        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(serviceUrl))
        {
            throw new InvalidOperationException("Configurações do Cloudflare R2 ausentes no appsettings.");
        }

        var config = new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true,
            AuthenticationRegion = "us-east-1"
        };

        _s3Client = new AmazonS3Client(accessKey, secretKey, config);
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType)
    {
        var putRequest = new Amazon.S3.Model.PutObjectRequest
        {
            InputStream = fileStream,
            Key = fileName,
            BucketName = _bucketName,
            ContentType = contentType,
            DisablePayloadSigning = true
        };

        await _s3Client.PutObjectAsync(putRequest);

        var finalUrl = _publicUrl.EndsWith("/") ? $"{_publicUrl}{fileName}" : $"{_publicUrl}/{fileName}";
        return finalUrl;
    }
}
