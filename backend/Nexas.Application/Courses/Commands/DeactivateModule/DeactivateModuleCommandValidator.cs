using FluentValidation;

namespace Nexas.Application.Courses.Commands.DeactivateModule
{
    public class DeactivateModuleCommandValidator : AbstractValidator<DeactivateModuleCommand>
    {
        public DeactivateModuleCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("ID do módulo deve ser maior que 0.");
        }
    }
}
