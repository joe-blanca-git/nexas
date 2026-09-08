using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Nexas.Application.RLS.Commands.AssignRole;

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, string>
{
    private readonly INexasDbContext _context;

    public AssignRoleCommandHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            throw new Exception("Usuário não encontrado.");
        }

        AppRole? role = null;
        if (!string.IsNullOrEmpty(request.RoleId))
        {
            role = await _context.AppRoles.FirstOrDefaultAsync(r => r.RoleId == request.RoleId, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(request.RoleName))
        {
            role = await _context.AppRoles.FirstOrDefaultAsync(r => r.Name == request.RoleName, cancellationToken);
        }

        if (role == null)
        {
            throw new Exception("Role não encontrada.");
        }

        var existingAssignment = await _context.AppUserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == user.ExternalId && ur.RoleId == role.RoleId, cancellationToken);

        if (existingAssignment != null)
        {
            throw new Exception("Usuário já possui esta role vinculada.");
        }

        var userRole = AppUserRole.Create(user.ExternalId, role.RoleId);
        _context.AppUserRoles.Add(userRole);
        
        await _context.SaveChangesAsync(cancellationToken);

        return "Role vinculada com sucesso.";
    }
}
