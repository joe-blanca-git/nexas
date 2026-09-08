using MediatR;

namespace Nexas.Application.RLS.Commands.RemoveRole;

public class RemoveRoleCommand : IRequest<string>
{
    public int UserId { get; set; }
    public string? RoleId { get; set; }
    public string? RoleName { get; set; }
}
