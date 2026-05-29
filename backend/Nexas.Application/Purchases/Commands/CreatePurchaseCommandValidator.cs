using FluentValidation;

namespace Nexas.Application.Purchases.Commands;

/// <summary>
/// Validador para o comando de criação de compra.
/// </summary>
public class CreatePurchaseCommandValidator : AbstractValidator<CreatePurchaseCommand>
{
    public CreatePurchaseCommandValidator()
    {
        RuleFor(v => v.CourseId)
            .GreaterThan(0)
            .WithMessage("O CourseId deve ser um identificador válido.");

        RuleFor(v => v.Amount)
            .GreaterThan(0)
            .WithMessage("O valor da compra deve ser maior que zero.");

        RuleFor(v => v.PaymentMethod)
            .NotEmpty()
            .WithMessage("O método de pagamento é obrigatório.");
    }
}
