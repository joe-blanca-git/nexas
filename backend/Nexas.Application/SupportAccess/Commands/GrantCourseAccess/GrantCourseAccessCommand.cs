using MediatR;

namespace Nexas.Application.SupportAccess.Commands.GrantCourseAccess;

public class GrantCourseAccessCommand : IRequest<bool>
{
    public int UserId { get; set; }
    public int CourseId { get; set; }

    public GrantCourseAccessCommand(int userId, int courseId)
    {
        UserId = userId;
        CourseId = courseId;
    }
}
