using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Enums;

namespace Nexas.Application.Refunds.Queries;

public record RefundableSaleDto(
    int PurchaseId,
    string StudentName,
    string CourseName,
    decimal GrossValue,
    string PaymentStatus,
    double CourseProgress,
    DateTime PurchaseDate,
    string PaymentMethod,
    bool IsEligible
);

public record GetSalesForRefundQuery(string? SearchTerm) : IRequest<List<RefundableSaleDto>>;

public class GetSalesForRefundQueryHandler : IRequestHandler<GetSalesForRefundQuery, List<RefundableSaleDto>>
{
    private readonly INexasDbContext _context;

    public GetSalesForRefundQueryHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<List<RefundableSaleDto>> Handle(GetSalesForRefundQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Purchases
            .Include(p => p.User)
            .Include(p => p.Course).ThenInclude(c => c.Modules).ThenInclude(m => m.Lessons).ThenInclude(l => l.LessonViews)
            .Where(p => p.Status == PurchaseStatus.Approved && !_context.RefundRequests.Any(r => r.PurchaseId == p.Id))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(p => 
                (p.User.FullName != null && p.User.FullName.ToLower().Contains(term)) ||
                p.Course.Name.ToLower().Contains(term) ||
                (p.AsaasPaymentId != null && p.AsaasPaymentId.ToLower().Contains(term))
            );
        }

        var purchases = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = new List<RefundableSaleDto>();

        foreach (var p in purchases)
        {
            var totalLessons = p.Course.Modules.SelectMany(m => m.Lessons).Count(l => l.Active);
            var completedLessons = p.Course.Modules
                .SelectMany(m => m.Lessons)
                .SelectMany(l => l.LessonViews)
                .Count(lv => lv.UserId == p.UserId);

            double progress = totalLessons > 0 ? (completedLessons * 100.0) / totalLessons : 0;
            bool isEligible = progress <= 20 && p.CreatedAt >= DateTime.UtcNow.AddDays(-7);

            result.Add(new RefundableSaleDto(
                p.Id,
                p.User.FullName ?? "Desconhecido",
                p.Course.Name,
                p.Amount,
                "Confirmado",
                progress,
                p.CreatedAt,
                p.PaymentMethod,
                isEligible
            ));
        }

        return result;
    }
}
