using FluentValidation;

namespace Nexas.Application.Lessons.Commands.ToggleLessonView;

public class ToggleLessonViewCommandValidator : AbstractValidator<ToggleLessonViewCommand>
{
    public ToggleLessonViewCommandValidator()
    {
        RuleFor(v => v.LessonId)
            .GreaterThan(0).WithMessage("O LessonId deve ser maior que zero.");
    }
}
