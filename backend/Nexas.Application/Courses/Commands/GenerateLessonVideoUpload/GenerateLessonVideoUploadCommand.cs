using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.Common.Models.Bunny;
using Nexas.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.Courses.Commands.GenerateLessonVideoUpload
{
    public record GenerateLessonVideoUploadCommand : IRequest<VideoUploadInformationDto>
    {
        public int LessonId { get; init; }
    }

    public class GenerateLessonVideoUploadCommandHandler : IRequestHandler<GenerateLessonVideoUploadCommand, VideoUploadInformationDto>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;
        private readonly IBunnyVideoService _bunnyVideoService;

        public GenerateLessonVideoUploadCommandHandler(
            INexasDbContext context, 
            IUserContextService userContextService, 
            IBunnyVideoService bunnyVideoService)
        {
            _context = context;
            _userContextService = userContextService;
            _bunnyVideoService = bunnyVideoService;
        }

        public async Task<VideoUploadInformationDto> Handle(GenerateLessonVideoUploadCommand request, CancellationToken cancellationToken)
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

            var course = lesson.Module.Course;
            if (string.IsNullOrWhiteSpace(course.BunnyLibraryId) || !int.TryParse(course.BunnyLibraryId, out int libraryId))
                throw new InvalidOperationException("O curso não possui um LibraryId válido configurado no Bunny.");

            var collectionId = lesson.Module.BunnyCollectionId;
            if (string.IsNullOrWhiteSpace(collectionId))
                throw new InvalidOperationException("O módulo não possui um CollectionId válido no Bunny.");

            // Idempotency: Se já tiver um BunnyVideoId gerado, reutiliza
            if (!string.IsNullOrWhiteSpace(lesson.BunnyVideoId) && lesson.Status == LessonStatus.PendingUpload)
            {
                return await _bunnyVideoService.GenerateUploadInformationAsync(libraryId, lesson.BunnyVideoId, cancellationToken);
            }

            // Cria um novo vídeo no Bunny
            var videoResponse = await _bunnyVideoService.CreateVideoAsync(libraryId, collectionId, lesson.Name, cancellationToken);
            
            // Salva na Lesson
            lesson.BunnyVideoId = videoResponse.Guid;
            lesson.Status = LessonStatus.PendingUpload;
            
            await _context.SaveChangesAsync(cancellationToken);

            // Retorna as informações de upload
            return await _bunnyVideoService.GenerateUploadInformationAsync(libraryId, videoResponse.Guid, cancellationToken);
        }
    }
}
