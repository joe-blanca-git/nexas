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
    public record FinancialClosingDto(
        int Id,
        int TeacherId,
        string TeacherName,
        DateTime PeriodStart,
        DateTime PeriodEnd,
        decimal GrossRevenue,
        decimal BankFeesTotal,
        decimal NexasFeesTotal,
        decimal NetValue,
        decimal TotalToReceive,
        ClosingStatus Status,
        DateTime? PaymentDate,
        string? AsaasTransferId,
        string? PaymentReceiptUrl,
        DateTime CreatedAt
    );

    public record GetClosingsHistoryQuery(int? TeacherId = null) : IRequest<List<FinancialClosingDto>>;

    public class GetClosingsHistoryQueryHandler : IRequestHandler<GetClosingsHistoryQuery, List<FinancialClosingDto>>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public GetClosingsHistoryQueryHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<List<FinancialClosingDto>> Handle(GetClosingsHistoryQuery request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();
            var loggedTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.IdAgivys == currentUser.ExternalId, cancellationToken);

            if (loggedTeacher == null)
                return new List<FinancialClosingDto>();

            bool isAdmin = loggedTeacher.Role == "Admin";

            // If not admin, force teacherId to themselves
            int filterTeacherId = isAdmin && request.TeacherId.HasValue ? request.TeacherId.Value : loggedTeacher.Id;

            var query = _context.FinancialClosings
                .Include(fc => fc.Teacher)
                .AsQueryable();

            if (!isAdmin || (isAdmin && request.TeacherId.HasValue))
            {
                query = query.Where(fc => fc.TeacherId == filterTeacherId);
            }

            var closings = await query
                .OrderByDescending(fc => fc.CreatedAt)
                .Select(fc => new FinancialClosingDto(
                    fc.Id,
                    fc.TeacherId,
                    fc.Teacher.Name,
                    fc.PeriodStart,
                    fc.PeriodEnd,
                    fc.GrossRevenue,
                    fc.BankFeesTotal,
                    fc.NexasFeesTotal,
                    fc.NetValue,
                    fc.TotalToReceive,
                    fc.Status,
                    fc.PaymentDate,
                    fc.AsaasTransferId,
                    fc.PaymentReceiptUrl,
                    fc.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            return closings;
        }
    }
}
