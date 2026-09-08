using MediatR;
using System.Collections.Generic;

namespace Nexas.Application.Users.Queries.GetUsers;

public class GetUsersQuery : IRequest<List<UserDto>>
{
}
