using FluentValidation;

namespace Nexas.Application.Courses.Commands.UpdateLesson
{
    public class UpdateLessonCommandValidator : AbstractValidator<UpdateLessonCommand>
    {
        public UpdateLessonCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("ID da aula deve ser maior que 0.");

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

            RuleFor(x => x.BunnyVideoId)
                .MaximumLength(100)
                .WithMessage("ID do vídeo Bunny não pode ultrapassar 100 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.BunnyVideoId));
        }
    }
}
