using System.Threading.Tasks;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Infrastructure.Services;

public class DummyFileStorageService : IFileStorageService
{
    public Task<string> UploadAsync(string bucket, string fileName, byte[] content, string contentType)
        => Task.FromResult("dummy_path/" + fileName);

    public Task<string> GenerateTemporaryUrlAsync(string bucket, string path, int expireInMinutes)
        => Task.FromResult("https://dummy.url/" + path);

    public Task DeleteAsync(string bucket, string path)
        => Task.CompletedTask;
}
