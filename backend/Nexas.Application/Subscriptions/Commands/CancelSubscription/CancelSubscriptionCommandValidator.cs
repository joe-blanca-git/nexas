using FluentValidation;

namespace Nexas.Application.Subscriptions.Commands.CancelSubscription;

public class CancelSubscriptionCommandValidator : AbstractValidator<CancelSubscriptionCommand>
{
    public CancelSubscriptionCommandValidator()
    {
        RuleFor(x => x.SubscriptionId).GreaterThan(0);
    }
}
