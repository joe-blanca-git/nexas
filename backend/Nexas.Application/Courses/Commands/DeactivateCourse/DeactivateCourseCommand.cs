using MediatR;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Courses.Commands.DeactivateCourse
{
    /// <summary>
    /// Comando para inativar um curso sem deletá-lo do banco de dados.
    /// </summary>
    public record DeactivateCourseCommand : IRequest<Unit>
    {
        /// <summary>ID do curso a ser inativado.</summary>
        /// <example>1</example>
        public int Id { get; init; }
    }

    public class DeactivateCourseCommandHandler : IRequestHandler<DeactivateCourseCommand, Unit>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public DeactivateCourseCommandHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<Unit> Handle(DeactivateCourseCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();

            var course = await _context.Courses.FindAsync(new object[] { request.Id }, cancellationToken: cancellationToken);
            if (course == null)
                throw new InvalidOperationException($"Curso com ID {request.Id} não encontrado.");

            course.Active = false;
            course.UpdatedBy = currentUser.Id;

            _context.Courses.Update(course);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
