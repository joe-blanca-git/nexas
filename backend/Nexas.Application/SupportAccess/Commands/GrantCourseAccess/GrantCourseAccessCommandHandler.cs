using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;
using Nexas.Domain.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.SupportAccess.Commands.GrantCourseAccess;

public class GrantCourseAccessCommandHandler : IRequestHandler<GrantCourseAccessCommand, bool>
{
    private readonly INexasDbContext _context;

    public GrantCourseAccessCommandHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(GrantCourseAccessCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null) return false;

        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);
        if (course == null) return false;

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == request.UserId && e.CourseId == request.CourseId, cancellationToken);

        if (enrollment == null)
        {
            enrollment = Enrollment.Create(request.UserId, request.CourseId, EnrollmentOrigin.Manual);
            _context.Enrollments.Add(enrollment);
        }
        else if (!enrollment.Active)
        {
            enrollment.Activate();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
