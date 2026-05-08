using FluentValidation;

namespace Nexas.Application.Courses.Commands.DeleteLesson
{
    public class DeleteLessonCommandValidator : AbstractValidator<DeleteLessonCommand>
    {
        public DeleteLessonCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("ID da aula deve ser maior que 0.");
        }
    }
}
