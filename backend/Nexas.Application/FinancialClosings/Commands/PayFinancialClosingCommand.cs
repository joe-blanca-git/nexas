using MediatR;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Enums;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Nexas.Application.FinancialClosings.Commands
{
    public record PayFinancialClosingResult(bool Success, string Message);

    public class PayFinancialClosingCommand : IRequest<PayFinancialClosingResult>
    {
        public int ClosingId { get; set; }
        public string AsaasTransferId { get; set; } = string.Empty;
        public Stream? FileStream { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
    }

    public class PayFinancialClosingCommandHandler : IRequestHandler<PayFinancialClosingCommand, PayFinancialClosingResult>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;
        private readonly ICloudflareStorageService _cloudflareStorageService;

        public PayFinancialClosingCommandHandler(
            INexasDbContext context, 
            IUserContextService userContextService,
            ICloudflareStorageService cloudflareStorageService)
        {
            _context = context;
            _userContextService = userContextService;
            _cloudflareStorageService = cloudflareStorageService;
        }

        public async Task<PayFinancialClosingResult> Handle(PayFinancialClosingCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();
            var adminCheck = await _context.Teachers.FirstOrDefaultAsync(t => t.IdAgivys == currentUser.ExternalId, cancellationToken);

            if (adminCheck == null || adminCheck.Role != "Admin")
                return new PayFinancialClosingResult(false, "Apenas administradores podem dar baixa em fechamentos.");

            var closing = await _context.FinancialClosings.FindAsync(new object[] { request.ClosingId }, cancellationToken);

            if (closing == null)
                return new PayFinancialClosingResult(false, "Fechamento não encontrado.");

            if (closing.Status == ClosingStatus.Paid)
                return new PayFinancialClosingResult(false, "Este fechamento já foi pago.");

            string? fileUrl = null;

            if (request.FileStream != null && !string.IsNullOrEmpty(request.FileName) && !string.IsNullOrEmpty(request.ContentType))
            {
                var uniqueFileName = $"receipts/{Guid.NewGuid()}_{request.FileName}";
                fileUrl = await _cloudflareStorageService.UploadImageAsync(request.FileStream, uniqueFileName, request.ContentType);
            }

            closing.Pay(request.AsaasTransferId, fileUrl);

            _context.FinancialClosings.Update(closing);
            await _context.SaveChangesAsync(cancellationToken);

            return new PayFinancialClosingResult(true, "Pagamento registrado com sucesso.");
        }
    }
}
