using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

namespace Nexas.Application.Portal.Financial.Queries.GetMyTransactions;

public class GetMyTransactionsQueryHandler : IRequestHandler<GetMyTransactionsQuery, List<GetMyTransactionsResponseDto>>
{
    private readonly INexasDbContext _context;
    private readonly IUserContextService _userContextService;

    public GetMyTransactionsQueryHandler(INexasDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<List<GetMyTransactionsResponseDto>> Handle(GetMyTransactionsQuery request, CancellationToken cancellationToken)
    {
        var currentUser = await _userContextService.GetCurrentUserAsync();

        var purchases = await _context.Purchases
            .Where(p => p.UserId == currentUser.Id)
            .Select(p => new {
                Purchase = p,
                Course = p.Course,
                TotalLessons = p.Course != null ? p.Course.Modules.SelectMany(m => m.Lessons).Count(l => l.Active) : 0,
                CompletedLessons = p.Course != null ? p.Course.Modules.SelectMany(m => m.Lessons).SelectMany(l => l.LessonViews).Count(lv => lv.UserId == currentUser.Id) : 0
            })
            .OrderByDescending(x => x.Purchase.CreatedAt)
            .ToListAsync(cancellationToken);

        return purchases.Select(x => {
            double progress = x.TotalLessons > 0 ? (x.CompletedLessons * 100.0) / x.TotalLessons : 0;
            
            bool isRefundable = x.Purchase.Status == Domain.Enums.PurchaseStatus.Approved
                                && (DateTime.UtcNow <= x.Purchase.CreatedAt.AddDays(7))
                                && progress <= 20;

            return new GetMyTransactionsResponseDto
            {
                Id = x.Purchase.Id,
                Name = $"Compra do curso {x.Course?.Name}",
                Value = x.Purchase.Amount,
                PaymentMethod = x.Purchase.PaymentMethod,
                Status = x.Purchase.Status.ToString(),
                PaymentDate = x.Purchase.CreatedAt,
                TransactionCode = x.Purchase.AsaasPaymentId ?? $"NEXAS-{x.Purchase.Id}",
                CourseId = x.Purchase.CourseId,
                IsRefundable = isRefundable,
                Progress = progress
            };
        }).ToList();
    }
}
