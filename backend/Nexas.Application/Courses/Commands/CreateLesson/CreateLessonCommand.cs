using MediatR;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Nexas.Application.Courses.Commands.CreateLesson
{
    /// <summary>
    /// Comando para criação de uma nova aula em um módulo existente.
    /// </summary>
    public record CreateLessonCommand : IRequest<int>
    {
        /// <summary>ID do módulo ao qual a aula será adicionada.</summary>
        /// <example>1</example>
        public int ModuleId { get; init; }

        /// <summary>Nome da aula.</summary>
        /// <example>Aula 1: Introdução ao C#</example>
        public string Name { get; init; } = string.Empty;

        /// <summary>Descrição detalhada do conteúdo da aula.</summary>
        /// <example>Aprenda os conceitos fundamentais da linguagem C#.</example>
        public string? Description { get; init; }

        /// <summary>Duração estimada da aula em segundos.</summary>
        /// <example>600</example>
        public int? DurationSeconds { get; init; }
    }

    public class CreateLessonCommandHandler : IRequestHandler<CreateLessonCommand, int>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public CreateLessonCommandHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<int> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();

            // Verificar se o módulo existe e incluir o Course e seus CourseTeachers
            var module = await _context.Modules
                .Include(m => m.Course)
                .ThenInclude(c => c.CourseTeachers)
                .ThenInclude(ct => ct.Teacher)
                .FirstOrDefaultAsync(m => m.Id == request.ModuleId, cancellationToken);
                
            if (module == null)
                throw new InvalidOperationException($"Módulo com ID {request.ModuleId} não encontrado.");

            // Verificar se o usuário atual é professor do curso vinculado a este módulo
            bool isTeacherOfCourse = module.Course.CourseTeachers.Any(ct => ct.Teacher.IdAgivys == currentUser.ExternalId);
            if (!isTeacherOfCourse)
                throw new UnauthorizedAccessException("Você não tem permissão para adicionar aulas a este módulo/curso.");

            var lesson = Lesson.Create(
                request.Name,
                request.Description,
                request.DurationSeconds,
                currentUser.Id
            );

            lesson.ModuleId = request.ModuleId;

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync(cancellationToken);

            // Update Course WorkloadHours
            var course = await _context.Courses
                .Include(c => c.Modules)
                .ThenInclude(m => m.Lessons)
                .FirstOrDefaultAsync(c => c.Id == module.CourseId, cancellationToken);

            if (course != null)
            {
                var totalSeconds = course.Modules.SelectMany(m => m.Lessons).Sum(l => l.DurationSeconds ?? 0);
                course.WorkloadHours = totalSeconds / 3600;
                await _context.SaveChangesAsync(cancellationToken);
            }

            return lesson.Id;
        }
    }
}
