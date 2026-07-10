using FluentValidation;

namespace Nexas.Application.Portal.Courses.Commands.RateCourse;

public class RateCourseCommandValidator : AbstractValidator<RateCourseCommand>
{
    public RateCourseCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0).WithMessage("O id do curso é obrigatório.");

        RuleFor(x => x.Rate)
            .InclusiveBetween(1, 5).WithMessage("A nota deve ser entre 1 e 5.");
    }
}
