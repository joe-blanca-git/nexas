using FluentValidation;

namespace Nexas.Application.Courses.Commands.UpdateCourse
{
    public class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
    {
        public UpdateCourseCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("ID do curso deve ser maior que 0.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Nome do curso é obrigatório.")
                .MaximumLength(255)
                .WithMessage("Nome do curso não pode ultrapassar 255 caracteres.");

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .WithMessage("Descrição do curso não pode ultrapassar 2000 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.DescriptionSub)
                .MaximumLength(500)
                .WithMessage("Sub-descrição do curso não pode ultrapassar 500 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.DescriptionSub));

            RuleFor(x => x.Level)
                .MaximumLength(100)
                .WithMessage("Nível do curso não pode ultrapassar 100 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Level));

            RuleFor(x => x.PriceSingle)
                .GreaterThan(0)
                .WithMessage("Preço do curso deve ser maior que 0.")
                .When(x => x.PriceSingle.HasValue);

            RuleFor(x => x.ImgCoverLink)
                .MaximumLength(2000)
                .WithMessage("Link da imagem de capa não pode ultrapassar 2000 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.ImgCoverLink));

            RuleFor(x => x.BunnyLibraryId)
                .MaximumLength(100)
                .WithMessage("ID da biblioteca Bunny não pode ultrapassar 100 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.BunnyLibraryId));
        }
    }
}
