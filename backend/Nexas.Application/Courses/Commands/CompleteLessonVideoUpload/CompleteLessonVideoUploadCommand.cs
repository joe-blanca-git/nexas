using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.Courses.Commands.CompleteLessonVideoUpload
{
    public record CompleteLessonVideoUploadCommand : IRequest<bool>
    {
        public int LessonId { get; init; }
    }

    public class CompleteLessonVideoUploadCommandHandler : IRequestHandler<CompleteLessonVideoUploadCommand, bool>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;
        private readonly IBunnyVideoService _bunnyVideoService;

        public CompleteLessonVideoUploadCommandHandler(
            INexasDbContext context, 
            IUserContextService userContextService, 
            IBunnyVideoService bunnyVideoService)
        {
            _context = context;
            _userContextService = userContextService;
            _bunnyVideoService = bunnyVideoService;
        }

        public async Task<bool> Handle(CompleteLessonVideoUploadCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();

            var lesson = await _context.Lessons
                .Include(l => l.Module)
                .ThenInclude(m => m.Course)
                .ThenInclude(c => c.CourseTeachers)
                .ThenInclude(ct => ct.Teacher)
                .FirstOrDefaultAsync(l => l.Id == request.LessonId, cancellationToken);

            if (lesson == null)
                throw new InvalidOperationException($"Aula com ID {request.LessonId} não encontrada.");

            bool isTeacherOfCourse = lesson.Module.Course.CourseTeachers.Any(ct => ct.Teacher.IdAgivys == currentUser.ExternalId);
            if (!isTeacherOfCourse)
                throw new UnauthorizedAccessException("Você não tem permissão para alterar esta aula.");

            if (string.IsNullOrWhiteSpace(lesson.BunnyVideoId))
                throw new InvalidOperationException("Esta aula não possui um vídeo associado.");

            var course = lesson.Module.Course;
            if (string.IsNullOrWhiteSpace(course.BunnyLibraryId) || !int.TryParse(course.BunnyLibraryId, out int libraryId))
                throw new InvalidOperationException("O curso não possui um LibraryId válido configurado no Bunny.");

            // Verifica na Bunny se o vídeo realmente existe
            var videoStatus = await _bunnyVideoService.GetVideoStatusAsync(libraryId, lesson.BunnyVideoId, cancellationToken);
            
            // Note: Pode-se verificar o status retornado para garantir que está processando, mas o objetivo principal 
            // é mudar nossa base para Processing para indicar que o Frontend terminou de enviar.
            
            lesson.Status = LessonStatus.Processing;
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
