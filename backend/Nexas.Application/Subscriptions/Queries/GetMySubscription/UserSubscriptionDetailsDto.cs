using System;
using System.Collections.Generic;

namespace Nexas.Application.Subscriptions.Queries.GetMySubscription;

public class UserSubscriptionDetailsDto
{
    public string SubscriptionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? NextDueDate { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public List<SubscriptionChargeDto> LastCharges { get; set; } = new List<SubscriptionChargeDto>();
}

public class SubscriptionChargeDto
{
    public string ChargeId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PaymentDate { get; set; }
}
