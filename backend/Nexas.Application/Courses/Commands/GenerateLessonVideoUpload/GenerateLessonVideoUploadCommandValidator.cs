using FluentValidation;

namespace Nexas.Application.Courses.Commands.GenerateLessonVideoUpload
{
    public class GenerateLessonVideoUploadCommandValidator : AbstractValidator<GenerateLessonVideoUploadCommand>
    {
        public GenerateLessonVideoUploadCommandValidator()
        {
            RuleFor(v => v.LessonId).GreaterThan(0).WithMessage("LessonId deve ser maior que zero.");
        }
    }
}
