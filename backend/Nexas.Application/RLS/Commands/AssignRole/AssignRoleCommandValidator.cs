using FluentValidation;

namespace Nexas.Application.RLS.Commands.AssignRole;

public class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("O UserId é obrigatório.");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.RoleId) || !string.IsNullOrEmpty(x.RoleName))
            .WithMessage("É obrigatório informar RoleId ou RoleName.");
    }
}
