using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Teachers.Commands.UnassignTeacher
{
    public record UnassignTeacherResult(bool Success, string Message);

    public record UnassignTeacherCommand : IRequest<UnassignTeacherResult>
    {
        public int TeacherId { get; init; }
        public int CourseId { get; init; }
    }

    public class UnassignTeacherCommandHandler : IRequestHandler<UnassignTeacherCommand, UnassignTeacherResult>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public UnassignTeacherCommandHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<UnassignTeacherResult> Handle(UnassignTeacherCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();
            var loggedTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.IdAgivys == currentUser.ExternalId, cancellationToken);

            if (loggedTeacher == null || loggedTeacher.Role != "Admin")
            {
                return new UnassignTeacherResult(false, "Apenas administradores podem desvincular professores.");
            }
            var courseTeacher = await _context.CourseTeachers
                .FirstOrDefaultAsync(ct => ct.TeacherId == request.TeacherId && ct.CourseId == request.CourseId, cancellationToken);

            if (courseTeacher == null)
                return new UnassignTeacherResult(false, "O vínculo entre o professor e o curso não foi encontrado.");

            _context.CourseTeachers.Remove(courseTeacher);
            await _context.SaveChangesAsync(cancellationToken);

            return new UnassignTeacherResult(true, "Professor desvinculado com sucesso.");
        }
    }
}
