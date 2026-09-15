using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexas.SystemManager.Domain.Entities;
using Nexas.SystemManager.Features.Applications.DTOs;
using Nexas.SystemManager.Infrastructure;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Nexas.SystemManager.Controllers
{
    /// <summary>
    /// Gerencia o cadastro de "Applications": os sistemas/apps registrados na plataforma Nexas
    /// (nome, domínio, logo e cores usados para customizar cada app cliente).
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "Admin,Owner")]
    public class ApplicationsController : ControllerBase
    {
        private readonly SystemManagerDbContext _context;

        public ApplicationsController(SystemManagerDbContext context)
        {
            _context = context;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system-user";
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Cria uma nova Application",
            Description = "Cadastra um novo sistema/app na plataforma Nexas.")]
        [ProducesResponseType(typeof(AppResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AppCreateDto request)
        {
            var app = new SystemApp
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                UrlLogo = request.UrlLogo,
                PrimaryColor = request.PrimaryColor,
                SecondaryColor = request.SecondaryColor,
                UrlDomain = request.UrlDomain,
                CreatedAt = DateTime.UtcNow,
                CreatedUser = GetCurrentUserId()
            };

            _context.Applications.Add(app);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = app.Id }, MapToResponse(app));
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Lista todas as Applications",
            Description = "Retorna todos os sistemas/apps cadastrados na plataforma.")]
        [ProducesResponseType(typeof(List<AppResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var apps = await _context.Applications.ToListAsync();
            return Ok(apps.Select(MapToResponse));
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Obtém uma Application pelo ID",
            Description = "Retorna os detalhes de um sistema/app específico pelo seu ID.")]
        [ProducesResponseType(typeof(AppResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var app = await _context.Applications.FindAsync(id);
            if (app == null) return NotFound();

            return Ok(MapToResponse(app));
        }

        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "Atualiza uma Application (substituição completa)",
            Description = "Substitui todos os campos de um sistema/app existente pelo ID.")]
        [ProducesResponseType(typeof(AppResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromBody] AppCreateDto request)
        {
            var app = await _context.Applications.FindAsync(id);
            if (app == null) return NotFound();

            app.Name = request.Name;
            app.Description = request.Description;
            app.UrlLogo = request.UrlLogo;
            app.PrimaryColor = request.PrimaryColor;
            app.SecondaryColor = request.SecondaryColor;
            app.UrlDomain = request.UrlDomain;
            app.UpdatedAt = DateTime.UtcNow;
            app.UpdatedUser = GetCurrentUserId();

            await _context.SaveChangesAsync();

            return Ok(MapToResponse(app));
        }

        [HttpPatch("{id}")]
        [SwaggerOperation(
            Summary = "Atualiza parcialmente uma Application",
            Description = "Atualiza somente os campos informados de um sistema/app existente pelo ID. Campos omitidos permanecem inalterados.")]
        [ProducesResponseType(typeof(AppResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Patch(Guid id, [FromBody] AppUpdateDto request)
        {
            var app = await _context.Applications.FindAsync(id);
            if (app == null) return NotFound();

            if (request.Name != null) app.Name = request.Name;
            if (request.Description != null) app.Description = request.Description;
            if (request.UrlLogo != null) app.UrlLogo = request.UrlLogo;
            if (request.PrimaryColor != null) app.PrimaryColor = request.PrimaryColor;
            if (request.SecondaryColor != null) app.SecondaryColor = request.SecondaryColor;
            if (request.UrlDomain != null) app.UrlDomain = request.UrlDomain;

            app.UpdatedAt = DateTime.UtcNow;
            app.UpdatedUser = GetCurrentUserId();

            await _context.SaveChangesAsync();

            return Ok(MapToResponse(app));
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Remove uma Application",
            Description = "Exclui definitivamente um sistema/app da plataforma pelo ID.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var app = await _context.Applications.FindAsync(id);
            if (app == null) return NotFound();

            _context.Applications.Remove(app);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static AppResponseDto MapToResponse(SystemApp app)
        {
            return new AppResponseDto
            {
                Id = app.Id,
                Name = app.Name,
                Description = app.Description,
                UrlLogo = app.UrlLogo,
                PrimaryColor = app.PrimaryColor,
                SecondaryColor = app.SecondaryColor,
                UrlDomain = app.UrlDomain,
                CreatedAt = app.CreatedAt,
                CreatedUser = app.CreatedUser,
                UpdatedAt = app.UpdatedAt,
                UpdatedUser = app.UpdatedUser
            };
        }
    }
}
