using MediatR;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

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

        /// <summary>ID do vídeo Bunny para a aula.</summary>
        /// <example>video_12345</example>
        public string? BunnyVideoId { get; init; }
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

            // Verificar se o módulo existe
            var module = await _context.Modules.FindAsync(new object[] { request.ModuleId }, cancellationToken: cancellationToken);
            if (module == null)
                throw new InvalidOperationException($"Módulo com ID {request.ModuleId} não encontrado.");

            var lesson = Lesson.Create(
                request.Name,
                request.Description,
                request.DurationSeconds,
                request.BunnyVideoId,
                currentUser.Id
            );

            lesson.ModuleId = request.ModuleId;

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync(cancellationToken);

            return lesson.Id;
        }
    }
}
