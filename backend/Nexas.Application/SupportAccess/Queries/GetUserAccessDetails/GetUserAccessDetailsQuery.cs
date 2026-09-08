using MediatR;

namespace Nexas.Application.SupportAccess.Queries.GetUserAccessDetails;

public class GetUserAccessDetailsQuery : IRequest<List<SupportCourseAccessDto>>
{
    public int UserId { get; set; }

    public GetUserAccessDetailsQuery(int userId)
    {
        UserId = userId;
    }
}
