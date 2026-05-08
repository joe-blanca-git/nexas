using FluentValidation;

namespace Nexas.Application.Courses.Commands.UpdateModule
{
    public class UpdateModuleCommandValidator : AbstractValidator<UpdateModuleCommand>
    {
        public UpdateModuleCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("ID do módulo deve ser maior que 0.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Nome do módulo é obrigatório.")
                .MaximumLength(255)
                .WithMessage("Nome do módulo não pode ultrapassar 255 caracteres.");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Descrição do módulo não pode ultrapassar 1000 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.DescriptionSub)
                .MaximumLength(500)
                .WithMessage("Sub-descrição do módulo não pode ultrapassar 500 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.DescriptionSub));

            RuleFor(x => x.ImgCoverLink)
                .MaximumLength(2000)
                .WithMessage("Link da imagem de capa não pode ultrapassar 2000 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.ImgCoverLink));

            RuleFor(x => x.BunnyCollectionId)
                .MaximumLength(100)
                .WithMessage("ID da coleção Bunny não pode ultrapassar 100 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.BunnyCollectionId));
        }
    }
}
