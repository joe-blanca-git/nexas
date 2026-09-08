using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.Courses.Commands.DeleteModule
{
    /// <summary>
    /// Comando para deletar permanentemente um módulo e todas as suas aulas.
    /// </summary>
    public record DeleteModuleCommand : IRequest<Unit>
    {
        /// <summary>ID do módulo a ser deletado.</summary>
        /// <example>1</example>
        public int Id { get; init; }
    }

    public class DeleteModuleCommandHandler : IRequestHandler<DeleteModuleCommand, Unit>
    {
        private readonly INexasDbContext _context;
        private readonly IBunnyVideoService _bunnyVideoService;
        private readonly ICloudflareStorageService _cloudflareStorageService;
        private readonly IUserContextService _userContextService;

        public DeleteModuleCommandHandler(INexasDbContext context, IBunnyVideoService bunnyVideoService, ICloudflareStorageService cloudflareStorageService, IUserContextService userContextService)
        {
            _context = context;
            _bunnyVideoService = bunnyVideoService;
            _cloudflareStorageService = cloudflareStorageService;
            _userContextService = userContextService;
        }

        public async Task<Unit> Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();

            var module = await _context.Modules
                .Include(m => m.Lessons)
                .Include(m => m.Course)
                .ThenInclude(c => c.CourseTeachers)
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (module == null)
                throw new InvalidOperationException($"Módulo com ID {request.Id} não encontrado.");

            var currentTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.IdAgivys == currentUser.ExternalId, cancellationToken);
            if (currentTeacher == null || (currentTeacher.Role != "Admin" && !module.Course.CourseTeachers.Any(ct => ct.TeacherId == currentTeacher.Id)))
            {
                throw new UnauthorizedAccessException("Você não tem permissão para deletar este módulo.");
            }

            // Apagar vídeos das aulas no Bunny.net
            if (module.Lessons.Any() && module.Course != null && !string.IsNullOrEmpty(module.Course.BunnyLibraryId))
            {
                if (int.TryParse(module.Course.BunnyLibraryId, out int libraryId))
                {
                    foreach (var lesson in module.Lessons)
                    {
                        if (!string.IsNullOrEmpty(lesson.BunnyVideoId))
                        {
                            try
                            {
                                await _bunnyVideoService.DeleteVideoAsync(libraryId, lesson.BunnyVideoId, cancellationToken);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Erro ao excluir vídeo do Bunny na aula {lesson.Id}: {ex.Message}");
                            }
                        }
                    }
                }
            }

            // Remover a imagem de capa do módulo do S3/Cloudflare
            if (!string.IsNullOrEmpty(module.ImgCoverLink))
            {
                await _cloudflareStorageService.DeleteImageAsync(module.ImgCoverLink);
            }

            // Remover todas as aulas do módulo
            _context.Lessons.RemoveRange(module.Lessons);

            // Remover o módulo
            _context.Modules.Remove(module);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
