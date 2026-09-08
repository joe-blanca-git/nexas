using MediatR;

namespace Nexas.Application.SupportAccess.Commands.RevokeCourseAccess;

public class RevokeCourseAccessCommand : IRequest<bool>
{
    public int UserId { get; set; }
    public int CourseId { get; set; }

    public RevokeCourseAccessCommand(int userId, int courseId)
    {
        UserId = userId;
        CourseId = courseId;
    }
}
