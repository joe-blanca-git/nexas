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
            AuthenticationRegion = "auto"
        };

        _s3Client = new AmazonS3Client(accessKey, secretKey, config);
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType)
    {
        // Ao copiar para um MemoryStream, garantimos que o SDK da AWS saiba o tamanho exato do stream e possa buscar (seek).
        // Isso permite que o SDK calcule o hash SHA256 do arquivo inteiro antes de enviar.
        // Assim, ele faz a assinatura completa do payload de uma vez, evitando o uso de assinaturas particionadas
        // (STREAMING-AWS4-HMAC-SHA256-PAYLOAD-TRAILER), que o Cloudflare R2 não suporta.
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var putRequest = new Amazon.S3.Model.PutObjectRequest
        {
            InputStream = memoryStream,
            Key = fileName,
            BucketName = _bucketName,
            ContentType = contentType
            // Não desabilitamos o DisablePayloadSigning porque as credenciais atuais requerem payload assinado (senão dá Unauthorized).
        };

        await _s3Client.PutObjectAsync(putRequest);

        var finalUrl = _publicUrl.EndsWith("/") ? $"{_publicUrl}{fileName}" : $"{_publicUrl}/{fileName}";
        return finalUrl;
    }
}
