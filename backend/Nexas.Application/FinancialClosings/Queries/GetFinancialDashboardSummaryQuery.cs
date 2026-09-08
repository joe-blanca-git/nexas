using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Enums;

namespace Nexas.Application.FinancialClosings.Queries
{
    public record FinancialDashboardSummaryDto(
        decimal TotalAvailable,
        decimal TotalPending,
        decimal TotalWithdrawn
    );

    public record GetFinancialDashboardSummaryQuery : IRequest<FinancialDashboardSummaryDto>;

    public class GetFinancialDashboardSummaryQueryHandler : IRequestHandler<GetFinancialDashboardSummaryQuery, FinancialDashboardSummaryDto>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public GetFinancialDashboardSummaryQueryHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<FinancialDashboardSummaryDto> Handle(GetFinancialDashboardSummaryQuery request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.IdAgivys == currentUser.ExternalId, cancellationToken);

            // Get active taxes
            var activeTaxes = await _context.FinancialTaxes
                .Where(t => t.IsActive && t.EffectiveFrom <= DateTime.UtcNow)
                .GroupBy(t => t.Type)
                .Select(g => g.OrderByDescending(t => t.EffectiveFrom).FirstOrDefault())
                .ToListAsync(cancellationToken);

            decimal nexasTax = activeTaxes.FirstOrDefault(t => t?.Type == TaxType.Nexas)?.Percentage ?? 0m;
            decimal pixTax = activeTaxes.FirstOrDefault(t => t?.Type == TaxType.Pix)?.Percentage ?? 0m;
            decimal creditCardTax = activeTaxes.FirstOrDefault(t => t?.Type == TaxType.CreditCard)?.Percentage ?? 0m;

            var thresholdDate = DateTime.UtcNow.AddDays(-7);

            if (teacher == null)
            {
                // Global summary for Admin (all unbilled approved purchases across the platform)
                var globalUnbilledPurchases = await _context.Purchases
                    .Where(p => p.Status == PurchaseStatus.Approved && 
                                !_context.FinancialClosingItems.Any(fci => fci.PurchaseId == p.Id))
                    .ToListAsync(cancellationToken);

                decimal globalAvailable = 0;
                decimal globalPending = 0;

                foreach (var purchase in globalUnbilledPurchases)
                {
                    decimal bankTax = purchase.PaymentMethod == "PIX" ? pixTax : creditCardTax;
                    decimal bankFeeValue = purchase.Amount * (bankTax / 100m);
                    decimal nexasFeeValue = purchase.Amount * (nexasTax / 100m);
                    decimal netValue = purchase.Amount - bankFeeValue - nexasFeeValue;

                    if (purchase.CreatedAt <= thresholdDate)
                    {
                        globalAvailable += netValue;
                    }
                    else
                    {
                        globalPending += netValue;
                    }
                }

                decimal globalWithdrawn = await _context.FinancialClosings
                    .Where(fc => fc.Status == ClosingStatus.Paid)
                    .SumAsync(fc => fc.TotalToReceive, cancellationToken);

                return new FinancialDashboardSummaryDto(
                    Math.Round(globalAvailable, 2),
                    Math.Round(globalPending, 2),
                    Math.Round(globalWithdrawn, 2)
                );
            }

            // Fetch unbilled purchases (not in FinancialClosingItems) for courses where the teacher has a quota
            var unbilledPurchases = await _context.Purchases
                .Include(p => p.Course)
                .ThenInclude(c => c.CourseTeachers)
                .Where(p => p.Status == PurchaseStatus.Approved && 
                            !_context.FinancialClosingItems.Any(fci => fci.PurchaseId == p.Id && fci.FinancialClosing.TeacherId == teacher.Id))
                .Where(p => p.Course.CourseTeachers.Any(ct => ct.TeacherId == teacher.Id))
                .ToListAsync(cancellationToken);

            decimal totalAvailable = 0;
            decimal totalPending = 0;

            foreach (var purchase in unbilledPurchases)
            {
                var courseTeacher = purchase.Course.CourseTeachers.First(ct => ct.TeacherId == teacher.Id);
                decimal teacherPercentage = courseTeacher.ParticipationPercentage / 100m;

                decimal bankTax = purchase.PaymentMethod == "PIX" ? pixTax : creditCardTax;

                decimal bankFeeValue = purchase.Amount * (bankTax / 100m);
                decimal nexasFeeValue = purchase.Amount * (nexasTax / 100m);
                decimal netValue = purchase.Amount - bankFeeValue - nexasFeeValue;

                decimal teacherValue = netValue * teacherPercentage;

                if (purchase.CreatedAt <= thresholdDate)
                {
                    totalAvailable += teacherValue;
                }
                else
                {
                    totalPending += teacherValue;
                }
            }

            // Get Total Withdrawn
            decimal totalWithdrawn = await _context.FinancialClosings
                .Where(fc => fc.TeacherId == teacher.Id && fc.Status == ClosingStatus.Paid)
                .SumAsync(fc => fc.TotalToReceive, cancellationToken);

            return new FinancialDashboardSummaryDto(
                Math.Round(totalAvailable, 2),
                Math.Round(totalPending, 2),
                Math.Round(totalWithdrawn, 2)
            );
        }
    }
}
