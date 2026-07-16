using FluentValidation;

namespace Nexas.Application.Courses.Commands.CompleteLessonVideoUpload
{
    public class CompleteLessonVideoUploadCommandValidator : AbstractValidator<CompleteLessonVideoUploadCommand>
    {
        public CompleteLessonVideoUploadCommandValidator()
        {
            RuleFor(v => v.LessonId).GreaterThan(0).WithMessage("LessonId deve ser maior que zero.");
        }
    }
}
