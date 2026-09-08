using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Enums;

namespace Nexas.Application.Refunds.Queries;

public record RefundRequestDto(
    int Id,
    string RequestCode,
    DateTime RequestDate,
    string StudentName,
    string CourseName,
    string TransactionCode,
    decimal PurchaseValue,
    double CourseProgress,
    string Status,
    string? SupportTicketCode,
    string PaymentMethod,
    bool IsEligible
);

public record GetRefundsQuery(string? Status, string? SearchTerm, DateTime? StartDate, DateTime? EndDate) : IRequest<List<RefundRequestDto>>;

public class GetRefundsQueryHandler : IRequestHandler<GetRefundsQuery, List<RefundRequestDto>>
{
    private readonly INexasDbContext _context;

    public GetRefundsQueryHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<List<RefundRequestDto>> Handle(GetRefundsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.RefundRequests
            .Include(r => r.Purchase).ThenInclude(p => p.User)
            .Include(r => r.Purchase).ThenInclude(p => p.Course).ThenInclude(c => c.Modules).ThenInclude(m => m.Lessons).ThenInclude(l => l.LessonViews)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Status) && request.Status != "Todos")
        {
            if (Enum.TryParse<RefundStatus>(request.Status, true, out var statusEnum))
            {
                query = query.Where(r => r.Status == statusEnum);
            }
        }

        if (request.StartDate.HasValue)
            query = query.Where(r => r.CreatedAt >= request.StartDate.Value);

        if (request.EndDate.HasValue)
        {
            var end = request.EndDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(r => r.CreatedAt <= end);
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(r => 
                r.RequestCode.ToLower().Contains(term) ||
                (r.Purchase.User.FullName != null && r.Purchase.User.FullName.ToLower().Contains(term)) ||
                r.Purchase.Course.Name.ToLower().Contains(term)
            );
        }

        var refunds = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = new List<RefundRequestDto>();

        foreach (var refund in refunds)
        {
            // Calculate progress
            var totalLessons = refund.Purchase.Course.Modules.SelectMany(m => m.Lessons).Count(l => l.Active);
            var completedLessons = refund.Purchase.Course.Modules
                .SelectMany(m => m.Lessons)
                .SelectMany(l => l.LessonViews)
                .Count(lv => lv.UserId == refund.Purchase.UserId);

            double progress = totalLessons > 0 ? (completedLessons * 100.0) / totalLessons : 0;
            bool isEligible = progress <= 20 && refund.Purchase.CreatedAt >= DateTime.UtcNow.AddDays(-7);

            string statusTranslation = refund.Status switch
            {
                RefundStatus.Pending => "Pendente",
                RefundStatus.Approved => "Aprovado",
                RefundStatus.Rejected => "Reprovado",
                RefundStatus.Processing => "Processando",
                RefundStatus.Refunded => "Reembolsado",
                RefundStatus.Failed => "Falha",
                _ => refund.Status.ToString()
            };

            result.Add(new RefundRequestDto(
                refund.Id,
                refund.RequestCode,
                refund.CreatedAt,
                refund.Purchase.User.FullName ?? "Desconhecido",
                refund.Purchase.Course.Name,
                refund.Purchase.AsaasPaymentId ?? "",
                refund.Purchase.Amount,
                progress,
                statusTranslation,
                refund.SupportTicketCode,
                refund.Purchase.PaymentMethod,
                isEligible
            ));
        }

        return result;
    }
}
