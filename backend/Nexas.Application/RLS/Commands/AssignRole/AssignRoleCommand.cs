using MediatR;

namespace Nexas.Application.RLS.Commands.AssignRole;

public class AssignRoleCommand : IRequest<string>
{
    public int UserId { get; set; }
    public string? RoleId { get; set; }
    public string? RoleName { get; set; }
}
