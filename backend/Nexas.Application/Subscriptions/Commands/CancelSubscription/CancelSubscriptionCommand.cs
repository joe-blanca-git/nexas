using MediatR;

namespace Nexas.Application.Subscriptions.Commands.CancelSubscription;

public record CancelSubscriptionCommand(int SubscriptionId) : IRequest<CancelSubscriptionResponseDto>;
