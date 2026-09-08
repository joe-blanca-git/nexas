using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

namespace Nexas.Application.Teachers.Commands.AssignTeacher
{
    public record AssignTeacherResult(bool Success, string Message);

    public record AssignTeacherCommand : IRequest<AssignTeacherResult>
    {
        public int TeacherId { get; init; }
        public int CourseId { get; init; }
    }

    public class AssignTeacherCommandHandler : IRequestHandler<AssignTeacherCommand, AssignTeacherResult>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public AssignTeacherCommandHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<AssignTeacherResult> Handle(AssignTeacherCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();
            var loggedTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.IdAgivys == currentUser.ExternalId, cancellationToken);

            if (loggedTeacher == null || loggedTeacher.Role != "Admin")
            {
                return new AssignTeacherResult(false, "Apenas administradores podem vincular professores.");
            }
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == request.TeacherId, cancellationToken);
            if (teacher == null) 
                return new AssignTeacherResult(false, "O professor informado não existe.");
            if (!teacher.Active) 
                return new AssignTeacherResult(false, "O professor informado está inativo e não pode ser vinculado.");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);
            if (course == null) 
                return new AssignTeacherResult(false, "O curso informado não existe.");
            if (!course.Active) 
                return new AssignTeacherResult(false, "O curso informado está inativo e não pode receber professores.");

            var alreadyAssigned = await _context.CourseTeachers
                .AnyAsync(ct => ct.TeacherId == request.TeacherId && ct.CourseId == request.CourseId, cancellationToken);

            if (alreadyAssigned) return new AssignTeacherResult(true, "O professor já está vinculado a este curso.");

            var courseTeacher = new CourseTeacher
            {
                TeacherId = request.TeacherId,
                CourseId = request.CourseId
            };

            _context.CourseTeachers.Add(courseTeacher);
            await _context.SaveChangesAsync(cancellationToken);

            return new AssignTeacherResult(true, "Professor vinculado com sucesso.");
        }
    }
}
