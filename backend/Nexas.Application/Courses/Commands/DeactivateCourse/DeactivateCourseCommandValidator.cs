using FluentValidation;

namespace Nexas.Application.Courses.Commands.DeactivateCourse
{
    public class DeactivateCourseCommandValidator : AbstractValidator<DeactivateCourseCommand>
    {
        public DeactivateCourseCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("ID do curso deve ser maior que 0.");
        }
    }
}
