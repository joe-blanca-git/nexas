using FluentValidation;

namespace Nexas.Application.Courses.Commands.DeactivateLesson
{
    public class DeactivateLessonCommandValidator : AbstractValidator<DeactivateLessonCommand>
    {
        public DeactivateLessonCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("ID da aula deve ser maior que 0.");
        }
    }
}
