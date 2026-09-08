using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.Teachers.Commands.UpdateCourseTeachers
{
    public record UpdateCourseTeachersResult(bool Success, string Message);

    public record TeacherShareDto(int TeacherId, decimal ParticipationPercentage);

    public record UpdateCourseTeachersCommand : IRequest<UpdateCourseTeachersResult>
    {
        public int CourseId { get; init; }
        public List<TeacherShareDto> Teachers { get; init; } = new();
    }

    public class UpdateCourseTeachersCommandHandler : IRequestHandler<UpdateCourseTeachersCommand, UpdateCourseTeachersResult>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public UpdateCourseTeachersCommandHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<UpdateCourseTeachersResult> Handle(UpdateCourseTeachersCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();
            var loggedTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.IdAgivys == currentUser.ExternalId, cancellationToken);

            if (loggedTeacher == null || loggedTeacher.Role != "Admin")
            {
                return new UpdateCourseTeachersResult(false, "Apenas administradores podem atualizar o vínculo de professores.");
            }

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);
            if (course == null) 
                return new UpdateCourseTeachersResult(false, "O curso informado não existe.");

            if (request.Teachers == null || !request.Teachers.Any())
                return new UpdateCourseTeachersResult(false, "É necessário informar pelo menos um professor.");

            var sum = request.Teachers.Sum(t => t.ParticipationPercentage);
            if (sum != 100m)
                return new UpdateCourseTeachersResult(false, "A soma das cotas de participação deve ser exatamente 100%.");

            // Validate all teachers
            var teacherIds = request.Teachers.Select(t => t.TeacherId).Distinct().ToList();
            if (teacherIds.Count != request.Teachers.Count)
                return new UpdateCourseTeachersResult(false, "Há professores duplicados na lista.");

            var dbTeachers = await _context.Teachers.Where(t => teacherIds.Contains(t.Id)).ToListAsync(cancellationToken);
            if (dbTeachers.Count != teacherIds.Count)
                return new UpdateCourseTeachersResult(false, "Um ou mais professores informados não existem.");

            if (dbTeachers.Any(t => !t.Active))
                return new UpdateCourseTeachersResult(false, "Um ou mais professores informados estão inativos e não podem ser vinculados.");

            // Remove old links
            var existingLinks = await _context.CourseTeachers
                .Where(ct => ct.CourseId == request.CourseId)
                .ToListAsync(cancellationToken);
                
            _context.CourseTeachers.RemoveRange(existingLinks);

            // Add new links
            foreach (var teacherShare in request.Teachers)
            {
                _context.CourseTeachers.Add(new CourseTeacher
                {
                    CourseId = request.CourseId,
                    TeacherId = teacherShare.TeacherId,
                    ParticipationPercentage = teacherShare.ParticipationPercentage
                });
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new UpdateCourseTeachersResult(true, "Professores vinculados com sucesso.");
        }
    }
}
