using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.Subscriptions.Queries.GetMySubscription;

public class GetMySubscriptionQueryHandler : IRequestHandler<GetMySubscriptionQuery, UserSubscriptionDetailsDto?>
{
    private readonly INexasDbContext _context;
    private readonly IUserContextService _userContextService;

    public GetMySubscriptionQueryHandler(INexasDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<UserSubscriptionDetailsDto?> Handle(GetMySubscriptionQuery request, CancellationToken cancellationToken)
    {
        var currentUser = await _userContextService.GetCurrentUserAsync();

        var subscription = await _context.Subscriptions
            .Include(s => s.Payments)
            .Where(s => s.UserId == currentUser.Id)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription == null) return null;

        var lastPayment = subscription.Payments.OrderByDescending(p => p.BillingDate).FirstOrDefault();

        var nextDue = lastPayment?.BillingDate.HasValue == true ? lastPayment.BillingDate.Value.AddMonths(1) : subscription.StartDate?.AddMonths(1);

        var dto = new UserSubscriptionDetailsDto
        {
            SubscriptionId = subscription.AsaasSubscriptionId ?? subscription.Id.ToString(),
            Status = subscription.Status.ToString().ToUpper(),
            StartDate = subscription.StartDate,
            NextDueDate = nextDue,
            PlanName = subscription.PlanName,
            LastCharges = subscription.Payments
                .OrderByDescending(p => p.BillingDate)
                .Select(p => new SubscriptionChargeDto
                {
                    ChargeId = p.AsaasPaymentId ?? p.Id.ToString(),
                    Amount = p.Amount,
                    Status = p.Status.ToString().ToUpper(),
                    PaymentDate = p.BillingDate
                }).ToList()
        };

        return dto;
    }
}
