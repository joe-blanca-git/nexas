using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.Common.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nexas.Admin.Api.Controllers
{
    [ApiController]
    [Route("v1/api/[controller]")]
    public class UploadsController : ControllerBase
    {
        private readonly ICloudflareStorageService _cloudflareStorageService;

        public UploadsController(ICloudflareStorageService cloudflareStorageService)
        {
            _cloudflareStorageService = cloudflareStorageService;
        }

        [HttpPost("Image")]
        [Authorize]
        [RequestSizeLimit(3 * 1024 * 1024)] // 3MB limit (slightly above 2MB for safety margin)
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Nenhum arquivo enviado." });

            if (file.Length > 2 * 1024 * 1024)
                return BadRequest(new { message = "O arquivo excede o limite de 2MB." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Formato de imagem não suportado. Use JPG, PNG ou WEBP." });

            var uniqueFileName = $"courses/covers/{Guid.NewGuid()}{extension}";

            using var stream = file.OpenReadStream();
            
            try
            {
                var publicUrl = await _cloudflareStorageService.UploadImageAsync(stream, uniqueFileName, file.ContentType);
                return Ok(new { url = publicUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao fazer upload da imagem.", detail = ex.Message });
            }
        }
    }
}
