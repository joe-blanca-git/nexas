using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

namespace Nexas.Application.Teachers.Commands.AssignTeacher
{
    public record AssignTeacherCommand : IRequest<bool>
    {
        public int TeacherId { get; init; }
        public int CourseId { get; init; }
    }

    public class AssignTeacherCommandHandler : IRequestHandler<AssignTeacherCommand, bool>
    {
        private readonly INexasDbContext _context;

        public AssignTeacherCommandHandler(INexasDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(AssignTeacherCommand request, CancellationToken cancellationToken)
        {
            var teacherExists = await _context.Teachers.AnyAsync(t => t.Id == request.TeacherId && t.Active, cancellationToken);
            if (!teacherExists) return false;

            var courseExists = await _context.Courses.AnyAsync(c => c.Id == request.CourseId && c.Active, cancellationToken);
            if (!courseExists) return false;

            var alreadyAssigned = await _context.CourseTeachers
                .AnyAsync(ct => ct.TeacherId == request.TeacherId && ct.CourseId == request.CourseId, cancellationToken);

            if (alreadyAssigned) return true; // Already assigned, idempotent

            var courseTeacher = new CourseTeacher
            {
                TeacherId = request.TeacherId,
                CourseId = request.CourseId
            };

            _context.CourseTeachers.Add(courseTeacher);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
