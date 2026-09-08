using MediatR;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;
using Nexas.Domain.Enums;

namespace Nexas.Application.FinancialTaxes.Commands
{
    public record CreateFinancialTaxResult(bool Success, string Message);

    public record CreateFinancialTaxCommand : IRequest<CreateFinancialTaxResult>
    {
        public TaxType Type { get; init; }
        public decimal Percentage { get; init; }
        public DateTime EffectiveFrom { get; init; }
    }

    public class CreateFinancialTaxCommandHandler : IRequestHandler<CreateFinancialTaxCommand, CreateFinancialTaxResult>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public CreateFinancialTaxCommandHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<CreateFinancialTaxResult> Handle(CreateFinancialTaxCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();

            var adminCheck = _context.Teachers.FirstOrDefault(t => t.IdAgivys == currentUser.ExternalId);
            if (adminCheck == null || adminCheck.Role != "Admin")
                return new CreateFinancialTaxResult(false, "Apenas administradores podem criar taxas.");

            var tax = new FinancialTax(request.Type, request.Percentage, request.EffectiveFrom);
            
            _context.FinancialTaxes.Add(tax);
            await _context.SaveChangesAsync(cancellationToken);

            return new CreateFinancialTaxResult(true, "Taxa financeira criada com sucesso.");
        }
    }
}
