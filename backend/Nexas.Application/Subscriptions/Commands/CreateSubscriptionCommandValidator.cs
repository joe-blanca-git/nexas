using FluentValidation;

namespace Nexas.Application.Subscriptions.Commands;

/// <summary>
/// Validador para o comando de criação de assinatura.
/// </summary>
public class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionCommandValidator()
    {
        RuleFor(v => v.PlanName)
            .NotEmpty()
            .WithMessage("O nome do plano é obrigatório.");

        RuleFor(v => v.Amount)
            .GreaterThan(0)
            .WithMessage("O valor da assinatura deve ser maior que zero.");
    }
}
