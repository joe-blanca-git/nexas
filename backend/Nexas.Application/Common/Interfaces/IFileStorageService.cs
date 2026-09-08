using System.Threading.Tasks;

namespace Nexas.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(string bucket, string fileName, byte[] content, string contentType);
    Task DeleteAsync(string bucket, string path);
    Task<string> GenerateTemporaryUrlAsync(string bucket, string path, int expireInMinutes);
}
