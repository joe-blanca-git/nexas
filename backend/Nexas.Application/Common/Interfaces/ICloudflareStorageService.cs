using System.IO;
using System.Threading.Tasks;

namespace Nexas.Application.Common.Interfaces;

public interface ICloudflareStorageService
{
    /// <summary>
    /// Faz o upload de um arquivo para o bucket do Cloudflare R2 e retorna a URL pública gerada.
    /// </summary>
    /// <param name="fileStream">Stream do arquivo a ser enviado.</param>
    /// <param name="fileName">Nome único do arquivo.</param>
    /// <param name="contentType">MIME type da imagem (ex: image/jpeg).</param>
    /// <returns>A URL pública onde o arquivo ficou acessível.</returns>
    Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType);
}
