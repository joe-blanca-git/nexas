using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

namespace Nexas.Application.Purchases.Commands.RefundCourse;

public class RefundCourseCommandHandler : IRequestHandler<RefundCourseCommand, RefundCourseResponseDto>
{
    private readonly INexasDbContext _context;
    private readonly IAsaasService _asaasService;
    private readonly IUserContextService _userContextService;

    public RefundCourseCommandHandler(INexasDbContext context, IAsaasService asaasService, IUserContextService userContextService)
    {
        _context = context;
        _asaasService = asaasService;
        _userContextService = userContextService;
    }

    public async Task<RefundCourseResponseDto> Handle(RefundCourseCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _userContextService.GetCurrentUserAsync();
            var purchase = await _context.Purchases
                .FirstOrDefaultAsync(p => p.Id == request.PurchaseId && p.UserId == currentUser.Id, cancellationToken);

            if (purchase == null)
            {
                return new RefundCourseResponseDto
                {
                    Success = false,
                    Message = "Compra não encontrada."
                };
            }

            if (purchase.Status != Domain.Enums.PurchaseStatus.Approved)
            {
                return new RefundCourseResponseDto
                {
                    Success = false,
                    Message = "Compra não está aprovada para estorno."
                };
            }

            var deadline = purchase.CreatedAt.AddDays(7);
            if (DateTime.UtcNow > deadline)
            {
                return new RefundCourseResponseDto
                {
                    Success = false,
                    Message = "Prazo de estorno expirado."
                };
            }

            if (string.IsNullOrWhiteSpace(purchase.AsaasPaymentId))
            {
                return new RefundCourseResponseDto
                {
                    Success = false,
                    Message = "Pagamento Asaas não encontrado para esta compra."
                };
            }

            await _asaasService.RefundPaymentAsync(purchase.AsaasPaymentId, cancellationToken);

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == purchase.UserId && e.CourseId == purchase.CourseId, cancellationToken);

            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
            }

            purchase.Refund();
            await _context.SaveChangesAsync(cancellationToken);

            return new RefundCourseResponseDto
            {
                Success = true,
                Message = "Estorno processado e matrícula cancelada com sucesso."
            };
    }
}
