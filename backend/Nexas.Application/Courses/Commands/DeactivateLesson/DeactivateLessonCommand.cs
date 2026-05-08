using MediatR;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Courses.Commands.DeactivateLesson
{
    /// <summary>
    /// Comando para inativar uma aula sem deletá-la do banco de dados.
    /// </summary>
    public record DeactivateLessonCommand : IRequest<Unit>
    {
        /// <summary>ID da aula a ser inativada.</summary>
        /// <example>1</example>
        public int Id { get; init; }
    }

    public class DeactivateLessonCommandHandler : IRequestHandler<DeactivateLessonCommand, Unit>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public DeactivateLessonCommandHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<Unit> Handle(DeactivateLessonCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();

            var lesson = await _context.Lessons.FindAsync(new object[] { request.Id }, cancellationToken: cancellationToken);
            if (lesson == null)
                throw new InvalidOperationException($"Aula com ID {request.Id} não encontrada.");

            lesson.Active = false;
            lesson.UpdatedBy = currentUser.Id;

            _context.Lessons.Update(lesson);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
