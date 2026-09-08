using MediatR;
using System.Collections.Generic;

namespace Nexas.Application.RLS.Queries.GetRoles;

public class GetRolesQuery : IRequest<List<RoleDto>>
{
}
