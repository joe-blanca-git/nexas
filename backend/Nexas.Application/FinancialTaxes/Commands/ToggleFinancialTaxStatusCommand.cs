using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.FinancialTaxes.Commands
{
    public record ToggleFinancialTaxStatusResult(bool Success, string Message);

    public record ToggleFinancialTaxStatusCommand(int Id) : IRequest<ToggleFinancialTaxStatusResult>;

    public class ToggleFinancialTaxStatusCommandHandler : IRequestHandler<ToggleFinancialTaxStatusCommand, ToggleFinancialTaxStatusResult>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public ToggleFinancialTaxStatusCommandHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<ToggleFinancialTaxStatusResult> Handle(ToggleFinancialTaxStatusCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();

            var adminCheck = await _context.Teachers.FirstOrDefaultAsync(t => t.IdAgivys == currentUser.ExternalId, cancellationToken);
            if (adminCheck == null || adminCheck.Role != "Admin")
                return new ToggleFinancialTaxStatusResult(false, "Apenas administradores podem alterar o status de taxas.");

            var tax = await _context.FinancialTaxes.FindAsync(new object[] { request.Id }, cancellationToken);
            if (tax == null)
                return new ToggleFinancialTaxStatusResult(false, "Taxa não encontrada.");

            tax.ToggleStatus();
            
            _context.FinancialTaxes.Update(tax);
            await _context.SaveChangesAsync(cancellationToken);

            return new ToggleFinancialTaxStatusResult(true, "Status da taxa alterado com sucesso.");
        }
    }
}
