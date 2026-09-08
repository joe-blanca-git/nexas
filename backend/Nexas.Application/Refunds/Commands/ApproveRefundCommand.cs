using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Refunds.Commands;

public record ApproveRefundCommand(int RefundRequestId) : IRequest<bool>;

public class ApproveRefundCommandHandler : IRequestHandler<ApproveRefundCommand, bool>
{
    private readonly INexasDbContext _context;

    public ApproveRefundCommandHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ApproveRefundCommand request, CancellationToken cancellationToken)
    {
        var refund = await _context.RefundRequests
            .FirstOrDefaultAsync(r => r.Id == request.RefundRequestId, cancellationToken);

        if (refund == null || refund.Status != Domain.Enums.RefundStatus.Pending)
            throw new Exception("Solicitação não encontrada ou não está pendente.");

        refund.Approve();
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
