using MediatR;

namespace Nexas.Application.SupportAccess.Queries.GetSupportUsers;

public class GetSupportUsersQuery : IRequest<List<SupportUserDto>>
{
}
