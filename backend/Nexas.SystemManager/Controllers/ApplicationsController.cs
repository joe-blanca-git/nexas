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
using System.Security.Cryptography;
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
                Status = string.IsNullOrWhiteSpace(request.Status) ? "active" : request.Status,
                GoogleClientId = request.GoogleClientId,
                ApiKey = GenerateApiKey(),
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
            app.Status = string.IsNullOrWhiteSpace(request.Status) ? app.Status : request.Status;
            app.GoogleClientId = request.GoogleClientId;
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
            if (request.Status != null) app.Status = request.Status;
            if (request.GoogleClientId != null)
            {
                app.GoogleClientId = string.IsNullOrWhiteSpace(request.GoogleClientId) ? null : request.GoogleClientId.Trim();
            }

            app.UpdatedAt = DateTime.UtcNow;
            app.UpdatedUser = GetCurrentUserId();

            await _context.SaveChangesAsync();

            return Ok(MapToResponse(app));
        }

        [HttpPost("{id}/api-key/regenerate")]
        [SwaggerOperation(
            Summary = "Gera uma nova chave de API",
            Description = "Invalida a chave de API atual da aplicação e gera uma nova em seu lugar.")]
        [ProducesResponseType(typeof(AppResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegenerateApiKey(Guid id)
        {
            var app = await _context.Applications.FindAsync(id);
            if (app == null) return NotFound();

            app.ApiKey = GenerateApiKey();
            app.UpdatedAt = DateTime.UtcNow;
            app.UpdatedUser = GetCurrentUserId();

            await _context.SaveChangesAsync();

            return Ok(MapToResponse(app));
        }

        [HttpGet("{id}/users")]
        [SwaggerOperation(
            Summary = "Lista os usuários finais de uma Application",
            Description = "Retorna os usuários cadastrados na Application (via /api/v1/apps/auth/register) — os clientes finais dela, não os administradores do portal.")]
        [ProducesResponseType(typeof(List<AppEndUserSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsers(Guid id)
        {
            var appExists = await _context.Applications.AnyAsync(a => a.Id == id);
            if (!appExists) return NotFound();

            var users = await _context.AppEndUsers
                .Where(u => u.ApplicationId == id)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new AppEndUserSummaryDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    CreatedAt = u.CreatedAt,
                    Roles = u.UserRoles
                        .Select(ur => new AppRoleDto
                        {
                            Id = ur.AppRole.Id,
                            Name = ur.AppRole.Name,
                            CreatedAt = ur.AppRole.CreatedAt
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}/roles")]
        [SwaggerOperation(
            Summary = "Lista os papéis (roles) de uma Application",
            Description = "Retorna os papéis criados pelo dono da aplicação (ex.: Admin, Professor, Aluno) para os seus usuários finais.")]
        [ProducesResponseType(typeof(List<AppRoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRoles(Guid id)
        {
            var appExists = await _context.Applications.AnyAsync(a => a.Id == id);
            if (!appExists) return NotFound();

            var roles = await _context.AppRoles
                .Where(r => r.ApplicationId == id)
                .OrderBy(r => r.Name)
                .Select(r => new AppRoleDto { Id = r.Id, Name = r.Name, CreatedAt = r.CreatedAt })
                .ToListAsync();

            return Ok(roles);
        }

        [HttpPost("{id}/roles")]
        [SwaggerOperation(
            Summary = "Cria um novo papel (role) para uma Application",
            Description = "Cadastra um papel novo (ex.: Admin, Professor, Aluno) para os usuários finais dessa aplicação. O nome precisa ser único dentro da aplicação.")]
        [ProducesResponseType(typeof(AppRoleDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateRole(Guid id, [FromBody] CreateAppRoleDto request)
        {
            var appExists = await _context.Applications.AnyAsync(a => a.Id == id);
            if (!appExists) return NotFound();

            var name = request.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { Message = "O nome do papel é obrigatório." });
            }

            var alreadyExists = await _context.AppRoles.AnyAsync(r => r.ApplicationId == id && r.Name == name);
            if (alreadyExists)
            {
                return BadRequest(new { Message = "Já existe um papel com esse nome nessa aplicação." });
            }

            var role = new AppRole
            {
                Id = Guid.NewGuid(),
                ApplicationId = id,
                Name = name,
                CreatedAt = DateTime.UtcNow
            };

            _context.AppRoles.Add(role);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRoles), new { id }, new AppRoleDto { Id = role.Id, Name = role.Name, CreatedAt = role.CreatedAt });
        }

        [HttpDelete("{id}/roles/{roleId}")]
        [SwaggerOperation(
            Summary = "Remove um papel (role) de uma Application",
            Description = "Exclui definitivamente um papel — usuários que tinham esse papel deixam de tê-lo.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteRole(Guid id, Guid roleId)
        {
            var role = await _context.AppRoles.FirstOrDefaultAsync(r => r.Id == roleId && r.ApplicationId == id);
            if (role == null) return NotFound();

            _context.AppRoles.Remove(role);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/users/{userId}/roles/{roleId}")]
        [SwaggerOperation(
            Summary = "Atribui um papel a um usuário final",
            Description = "Associa um AppRole já existente a um AppEndUser. Um usuário pode acumular vários papéis.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AssignRole(Guid id, Guid userId, Guid roleId)
        {
            var role = await _context.AppRoles.FirstOrDefaultAsync(r => r.Id == roleId && r.ApplicationId == id);
            if (role == null) return NotFound();

            var user = await _context.AppEndUsers.FirstOrDefaultAsync(u => u.Id == userId && u.ApplicationId == id);
            if (user == null) return NotFound();

            var alreadyAssigned = await _context.AppEndUserRoles
                .AnyAsync(ur => ur.AppEndUserId == userId && ur.AppRoleId == roleId);
            if (!alreadyAssigned)
            {
                _context.AppEndUserRoles.Add(new AppEndUserRole
                {
                    AppEndUserId = userId,
                    AppRoleId = roleId,
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpDelete("{id}/users/{userId}/roles/{roleId}")]
        [SwaggerOperation(
            Summary = "Remove um papel de um usuário final",
            Description = "Desassocia um AppRole de um AppEndUser, sem excluir nem o papel nem o usuário.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveRole(Guid id, Guid userId, Guid roleId)
        {
            var link = await _context.AppEndUserRoles
                .FirstOrDefaultAsync(ur => ur.AppEndUserId == userId && ur.AppRoleId == roleId);
            if (link == null) return NotFound();

            _context.AppEndUserRoles.Remove(link);
            await _context.SaveChangesAsync();

            return NoContent();
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
                Status = app.Status,
                ApiKey = app.ApiKey,
                GoogleClientId = app.GoogleClientId,
                CreatedAt = app.CreatedAt,
                CreatedUser = app.CreatedUser,
                UpdatedAt = app.UpdatedAt,
                UpdatedUser = app.UpdatedUser
            };
        }

        private static string GenerateApiKey()
        {
            var bytes = RandomNumberGenerator.GetBytes(24);
            return "nxs_" + Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
