using MediatR;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Courses.Commands.UpdateLesson
{
    /// <summary>
    /// Comando para atualizar os dados de uma aula existente.
    /// </summary>
    public record UpdateLessonCommand : IRequest<Unit>
    {
        /// <summary>ID da aula a ser atualizada.</summary>
        /// <example>1</example>
        public int Id { get; init; }

        /// <summary>Novo nome da aula.</summary>
        /// <example>Aula 1: Introdução ao C# (Atualizada)</example>
        public string Name { get; init; } = string.Empty;

        /// <summary>Nova descrição da aula.</summary>
        public string? Description { get; init; }

        /// <summary>Nova duração estimada em segundos.</summary>
        public int? DurationSeconds { get; init; }

        /// <summary>Novo ID do vídeo Bunny para a aula.</summary>
        public string? BunnyVideoId { get; init; }
    }

    public class UpdateLessonCommandHandler : IRequestHandler<UpdateLessonCommand, Unit>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public UpdateLessonCommandHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<Unit> Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();

            var lesson = await _context.Lessons.FindAsync(new object[] { request.Id }, cancellationToken: cancellationToken);
            if (lesson == null)
                throw new InvalidOperationException($"Aula com ID {request.Id} não encontrada.");

            lesson.Name = request.Name;
            lesson.Description = request.Description;
            lesson.DurationSeconds = request.DurationSeconds;
            lesson.BunnyVideoId = request.BunnyVideoId;
            lesson.UpdatedBy = currentUser.Id;

            _context.Lessons.Update(lesson);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
