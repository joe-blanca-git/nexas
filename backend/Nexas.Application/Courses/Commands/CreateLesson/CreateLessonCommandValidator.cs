using FluentValidation;

namespace Nexas.Application.Courses.Commands.CreateLesson
{
    public class CreateLessonCommandValidator : AbstractValidator<CreateLessonCommand>
    {
        public CreateLessonCommandValidator()
        {
            RuleFor(x => x.ModuleId)
                .GreaterThan(0)
                .WithMessage("ID do módulo deve ser maior que 0.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Nome da aula é obrigatório.")
                .MaximumLength(255)
                .WithMessage("Nome da aula não pode ultrapassar 255 caracteres.");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Descrição da aula não pode ultrapassar 1000 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.DurationSeconds)
                .GreaterThan(0)
                .WithMessage("Duração da aula deve ser maior que 0 segundos.")
                .When(x => x.DurationSeconds.HasValue);


        }
    }
}
