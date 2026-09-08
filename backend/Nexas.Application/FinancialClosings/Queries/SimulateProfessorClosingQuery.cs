using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.FinancialClosings.Queries
{
    public record FinancialClosingItemSimulationDto(
        int PurchaseId,
        string CourseName,
        string StudentName,
        DateTime PurchaseDate,
        string PaymentMethod,
        decimal AppliedTeacherPercentage,
        decimal GrossValue,
        decimal BankFeeValue,
        decimal NexasFeeValue,
        decimal CalculatedValue
    );

    public record FinancialClosingSimulationDto(
        decimal GrossRevenue,
        decimal BankFeesTotal,
        decimal NexasFeesTotal,
        decimal NetValue,
        decimal TotalToReceive,
        List<FinancialClosingItemSimulationDto> Items
    );

    public record SimulateProfessorClosingQuery : IRequest<FinancialClosingSimulationDto>;

    public class SimulateProfessorClosingQueryHandler : IRequestHandler<SimulateProfessorClosingQuery, FinancialClosingSimulationDto>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public SimulateProfessorClosingQueryHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<FinancialClosingSimulationDto> Handle(SimulateProfessorClosingQuery request, CancellationToken cancellationToken)
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
                // Global simulation for Admin (all approved purchases across all courses)
                var allApprovedPurchases = await _context.Purchases
                    .Include(p => p.Course)
                    .ThenInclude(c => c.CourseTeachers)
                    .Include(p => p.User)
                    .Where(p => p.Status == PurchaseStatus.Approved)
                    .ToListAsync(cancellationToken);

                var globalItems = new List<FinancialClosingItemSimulationDto>();
                decimal gGrossRevenue = 0;
                decimal gBankFees = 0;
                decimal gNexasFees = 0;
                decimal gNetValue = 0;
                decimal gTotalToReceive = 0;

                foreach (var purchase in allApprovedPurchases)
                {
                    var firstTeacher = purchase.Course.CourseTeachers.FirstOrDefault();
                    decimal teacherPercentage = firstTeacher?.ParticipationPercentage ?? 100m;

                    decimal grossValue = purchase.Amount;
                    decimal bankFeeValue = purchase.PaymentMethod == "PIX" ? pixTax : grossValue * (creditCardTax / 100m);
                    decimal nexasFeeValue = grossValue * (nexasTax / 100m);
                    decimal netValue = grossValue - bankFeeValue - nexasFeeValue;
                    decimal calculatedTeacherValue = netValue * (teacherPercentage / 100m);

                    gGrossRevenue += grossValue;
                    gBankFees += bankFeeValue;
                    gNexasFees += nexasFeeValue;
                    gNetValue += netValue;
                    gTotalToReceive += calculatedTeacherValue;

                    globalItems.Add(new FinancialClosingItemSimulationDto(
                        purchase.Id,
                        purchase.Course.Name,
                        purchase.User.FullName ?? "Aluno",
                        purchase.CreatedAt,
                        purchase.PaymentMethod,
                        teacherPercentage,
                        grossValue,
                        bankFeeValue,
                        nexasFeeValue,
                        calculatedTeacherValue
                    ));
                }

                return new FinancialClosingSimulationDto(
                    Math.Round(gGrossRevenue, 2),
                    Math.Round(gBankFees, 2),
                    Math.Round(gNexasFees, 2),
                    Math.Round(gNetValue, 2),
                    Math.Round(gTotalToReceive, 2),
                    globalItems.OrderByDescending(i => i.PurchaseDate).ToList()
                );
            }

            // Fetch eligible unbilled purchases for a specific teacher
            var eligiblePurchases = await _context.Purchases
                .Include(p => p.Course)
                .ThenInclude(c => c.CourseTeachers)
                .Include(p => p.User)
                .Where(p => p.Status == PurchaseStatus.Approved && 
                            p.CreatedAt <= thresholdDate &&
                            !_context.FinancialClosingItems.Any(fci => fci.PurchaseId == p.Id && fci.FinancialClosing.TeacherId == teacher.Id))
                .Where(p => p.Course.CourseTeachers.Any(ct => ct.TeacherId == teacher.Id))
                .ToListAsync(cancellationToken);

            var items = new List<FinancialClosingItemSimulationDto>();
            
            decimal totalGrossRevenue = 0;
            decimal totalBankFees = 0;
            decimal totalNexasFees = 0;
            decimal totalNetValue = 0;
            decimal totalToReceive = 0;

            foreach (var purchase in eligiblePurchases)
            {
                var courseTeacher = purchase.Course.CourseTeachers.First(ct => ct.TeacherId == teacher.Id);
                decimal teacherPercentage = courseTeacher.ParticipationPercentage / 100m;

                decimal grossValue = purchase.Amount;
                decimal bankFeeValue = purchase.PaymentMethod == "PIX" ? pixTax : grossValue * (creditCardTax / 100m);
                decimal nexasFeeValue = grossValue * (nexasTax / 100m);
                decimal netValue = grossValue - bankFeeValue - nexasFeeValue;
                decimal calculatedTeacherValue = netValue * teacherPercentage;

                totalGrossRevenue += grossValue * teacherPercentage;
                totalBankFees += bankFeeValue * teacherPercentage;
                totalNexasFees += nexasFeeValue * teacherPercentage;
                totalNetValue += netValue * teacherPercentage;
                totalToReceive += calculatedTeacherValue;

                items.Add(new FinancialClosingItemSimulationDto(
                    purchase.Id,
                    purchase.Course.Name,
                    purchase.User.FullName ?? "Aluno",
                    purchase.CreatedAt,
                    purchase.PaymentMethod,
                    courseTeacher.ParticipationPercentage,
                    grossValue,
                    bankFeeValue,
                    nexasFeeValue,
                    calculatedTeacherValue
                ));
            }

            return new FinancialClosingSimulationDto(
                Math.Round(totalGrossRevenue, 2),
                Math.Round(totalBankFees, 2),
                Math.Round(totalNexasFees, 2),
                Math.Round(totalNetValue, 2),
                Math.Round(totalToReceive, 2),
                items.OrderByDescending(i => i.PurchaseDate).ToList()
            );
        }
    }
}
