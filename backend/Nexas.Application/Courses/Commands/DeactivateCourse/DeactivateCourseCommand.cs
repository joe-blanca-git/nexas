using MediatR;
using Nexas.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

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

            var course = await _context.Courses
                .Include(c => c.CourseTeachers)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
                
            if (course == null)
                throw new InvalidOperationException($"Curso com ID {request.Id} não encontrado.");

            var currentTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.IdAgivys == currentUser.ExternalId, cancellationToken);
            if (currentTeacher == null || (currentTeacher.Role != "Admin" && !course.CourseTeachers.Any(ct => ct.TeacherId == currentTeacher.Id)))
            {
                throw new UnauthorizedAccessException("Você não tem permissão para inativar este curso.");
            }

            course.Active = false;
            course.UpdatedBy = currentUser.Id;

            _context.Courses.Update(course);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
