using FluentValidation;

namespace Nexas.Application.Courses.Commands.CreateCourse
{
    public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
    {
        public CreateCourseCommandValidator()
        {
            RuleFor(v => v.Name)
                .NotEmpty().WithMessage("Course name is required.")
                .MaximumLength(255).WithMessage("Course name must not exceed 255 characters.");

            RuleFor(v => v.ImgCoverLink)
                .MaximumLength(2000).WithMessage("Course cover image URL must not exceed 2000 characters.")
                .When(v => !string.IsNullOrEmpty(v.ImgCoverLink));

            RuleFor(v => v.BunnyLibraryId)
                .MaximumLength(100).WithMessage("Course Bunny library ID must not exceed 100 characters.")
                .When(v => !string.IsNullOrEmpty(v.BunnyLibraryId));

            RuleForEach(v => v.Modules).ChildRules(modules =>
            {
                modules.RuleFor(m => m.Name)
                    .NotEmpty().WithMessage("Module name is required.")
                    .MaximumLength(255).WithMessage("Module name must not exceed 255 characters.");

                modules.RuleFor(m => m.ImgCoverLink)
                    .MaximumLength(2000).WithMessage("Module cover image URL must not exceed 2000 characters.")
                    .When(m => !string.IsNullOrEmpty(m.ImgCoverLink));

                modules.RuleFor(m => m.BunnyCollectionId)
                    .MaximumLength(100).WithMessage("Module Bunny collection ID must not exceed 100 characters.")
                    .When(m => !string.IsNullOrEmpty(m.BunnyCollectionId));

                modules.RuleForEach(m => m.Lessons).ChildRules(lessons =>
                {
                    lessons.RuleFor(l => l.Name)
                        .NotEmpty().WithMessage("Lesson name is required.")
                        .MaximumLength(255).WithMessage("Lesson name must not exceed 255 characters.");

                    lessons.RuleFor(l => l.BunnyVideoId)
                        .MaximumLength(100).WithMessage("Lesson Bunny video ID must not exceed 100 characters.")
                        .When(l => !string.IsNullOrEmpty(l.BunnyVideoId));
                });
            });
        }
    }
}
