using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Refunds.Commands;

public record RejectRefundCommand(int RefundRequestId, string Reason) : IRequest<bool>;

public class RejectRefundCommandHandler : IRequestHandler<RejectRefundCommand, bool>
{
    private readonly INexasDbContext _context;

    public RejectRefundCommandHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RejectRefundCommand request, CancellationToken cancellationToken)
    {
        var refund = await _context.RefundRequests
            .Include(r => r.Purchase)
            .FirstOrDefaultAsync(r => r.Id == request.RefundRequestId, cancellationToken);

        if (refund == null || refund.Status != Domain.Enums.RefundStatus.Pending)
            throw new Exception("Solicitação não encontrada ou não está pendente.");

        refund.Reject(request.Reason);
        
        // Reverte a compra para aprovada, pois o estorno foi negado
        refund.Purchase.Approve();
        
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
