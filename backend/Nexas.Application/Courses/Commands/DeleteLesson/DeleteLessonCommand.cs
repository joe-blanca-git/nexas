using MediatR;
using Nexas.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.Courses.Commands.DeleteLesson
{
    /// <summary>
    /// Comando para deletar permanentemente uma aula.
    /// </summary>
    public record DeleteLessonCommand : IRequest<Unit>
    {
        /// <summary>ID da aula a ser deletada.</summary>
        /// <example>1</example>
        public int Id { get; init; }
    }

    public class DeleteLessonCommandHandler : IRequestHandler<DeleteLessonCommand, Unit>
    {
        private readonly INexasDbContext _context;
        private readonly IBunnyVideoService _bunnyVideoService;
        private readonly IUserContextService _userContextService;

        public DeleteLessonCommandHandler(INexasDbContext context, IBunnyVideoService bunnyVideoService, IUserContextService userContextService)
        {
            _context = context;
            _bunnyVideoService = bunnyVideoService;
            _userContextService = userContextService;
        }

        public async Task<Unit> Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();

            var lesson = await _context.Lessons
                .Include(l => l.Module)
                .ThenInclude(m => m.Course)
                .ThenInclude(c => c.CourseTeachers)
                .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

            if (lesson == null)
                throw new InvalidOperationException($"Aula com ID {request.Id} não encontrada.");

            var currentTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.IdAgivys == currentUser.ExternalId, cancellationToken);
            if (currentTeacher == null || (currentTeacher.Role != "Admin" && !lesson.Module.Course.CourseTeachers.Any(ct => ct.TeacherId == currentTeacher.Id)))
            {
                throw new UnauthorizedAccessException("Você não tem permissão para deletar esta aula.");
            }


            if (!string.IsNullOrEmpty(lesson.BunnyVideoId) && !string.IsNullOrEmpty(lesson.Module?.Course?.BunnyLibraryId))
            {
                if (int.TryParse(lesson.Module.Course.BunnyLibraryId, out int libraryId))
                {
                    try
                    {
                        await _bunnyVideoService.DeleteVideoAsync(libraryId, lesson.BunnyVideoId, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao excluir vídeo do Bunny: {ex.Message}");
                    }
                }
            }

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
