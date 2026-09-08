using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.SupportAccess.Commands.RevokeCourseAccess;

public class RevokeCourseAccessCommandHandler : IRequestHandler<RevokeCourseAccessCommand, bool>
{
    private readonly INexasDbContext _context;

    public RevokeCourseAccessCommandHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RevokeCourseAccessCommand request, CancellationToken cancellationToken)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == request.UserId && e.CourseId == request.CourseId, cancellationToken);

        if (enrollment != null && enrollment.Active)
        {
            enrollment.Deactivate();
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        return false;
    }
}
