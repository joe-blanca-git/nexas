using MediatR;

namespace Nexas.Application.Subscriptions.Queries.GetMySubscription;

public record GetMySubscriptionQuery() : IRequest<UserSubscriptionDetailsDto?>;
