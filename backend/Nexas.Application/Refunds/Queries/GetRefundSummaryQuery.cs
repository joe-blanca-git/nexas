using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Enums;

namespace Nexas.Application.Refunds.Queries;

public record RefundDashboardSummaryDto(
    int TotalPending,
    int TotalApproved,
    int TotalProcessing,
    int TotalRefunded,
    decimal TotalRefundedValue
);

public record GetRefundSummaryQuery(DateTime? StartDate, DateTime? EndDate) : IRequest<RefundDashboardSummaryDto>;

public class GetRefundSummaryQueryHandler : IRequestHandler<GetRefundSummaryQuery, RefundDashboardSummaryDto>
{
    private readonly INexasDbContext _context;

    public GetRefundSummaryQueryHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<RefundDashboardSummaryDto> Handle(GetRefundSummaryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.RefundRequests
            .Include(r => r.Purchase)
            .AsQueryable();

        if (request.StartDate.HasValue)
            query = query.Where(r => r.CreatedAt >= request.StartDate.Value);

        if (request.EndDate.HasValue)
        {
            var end = request.EndDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(r => r.CreatedAt <= end);
        }

        var refunds = await query.ToListAsync(cancellationToken);

        int totalPending = refunds.Count(r => r.Status == RefundStatus.Pending);
        int totalApproved = refunds.Count(r => r.Status == RefundStatus.Approved);
        int totalProcessing = refunds.Count(r => r.Status == RefundStatus.Processing);
        int totalRefunded = refunds.Count(r => r.Status == RefundStatus.Refunded);
        
        decimal totalRefundedValue = refunds
            .Where(r => r.Status == RefundStatus.Refunded)
            .Sum(r => r.Purchase.Amount);

        return new RefundDashboardSummaryDto(
            totalPending,
            totalApproved,
            totalProcessing,
            totalRefunded,
            totalRefundedValue
        );
    }
}
