using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Enums;

namespace Nexas.Application.FinancialTaxes.Queries
{
    public record FinancialTaxDto(int Id, TaxType Type, decimal Percentage, DateTime EffectiveFrom, bool IsActive);

    public record GetFinancialTaxesQuery : IRequest<List<FinancialTaxDto>>;

    public class GetFinancialTaxesQueryHandler : IRequestHandler<GetFinancialTaxesQuery, List<FinancialTaxDto>>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public GetFinancialTaxesQueryHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<List<FinancialTaxDto>> Handle(GetFinancialTaxesQuery request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();
            
            var adminCheck = await _context.Teachers.FirstOrDefaultAsync(t => t.IdAgivys == currentUser.ExternalId, cancellationToken);
            if (adminCheck == null || adminCheck.Role != "Admin")
                throw new UnauthorizedAccessException("Apenas administradores podem visualizar as taxas.");

            var taxes = await _context.FinancialTaxes
                .OrderBy(t => t.Type)
                .ThenByDescending(t => t.EffectiveFrom)
                .Select(t => new FinancialTaxDto(
                    t.Id,
                    t.Type,
                    t.Percentage,
                    t.EffectiveFrom,
                    t.IsActive
                ))
                .ToListAsync(cancellationToken);

            return taxes;
        }
    }
}
