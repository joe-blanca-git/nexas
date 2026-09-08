using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Purchases.Commands.CancelPurchase;

public class CancelPurchaseCommandHandler : IRequestHandler<CancelPurchaseCommand, CancelPurchaseResponseDto>
{
    private readonly INexasDbContext _context;
    private readonly IAsaasService _asaasService;
    private readonly IUserContextService _userContextService;

    public CancelPurchaseCommandHandler(INexasDbContext context, IAsaasService asaasService, IUserContextService userContextService)
    {
        _context = context;
        _asaasService = asaasService;
        _userContextService = userContextService;
    }

    public async Task<CancelPurchaseResponseDto> Handle(CancelPurchaseCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _userContextService.GetCurrentUserAsync();
        var purchase = await _context.Purchases
            .FirstOrDefaultAsync(p => p.Id == request.PurchaseId && p.UserId == currentUser.Id, cancellationToken);

        if (purchase == null)
        {
            return new CancelPurchaseResponseDto
            {
                Success = false,
                Message = "Compra não encontrada."
            };
        }

        if (purchase.Status != Domain.Enums.PurchaseStatus.Pending && purchase.Status != Domain.Enums.PurchaseStatus.Approved)
        {
            return new CancelPurchaseResponseDto
            {
                Success = false,
                Message = "Compra não pode ser cancelada no status atual."
            };
        }

        if (!string.IsNullOrWhiteSpace(purchase.AsaasPaymentId))
        {
            try 
            {
                await _asaasService.CancelPaymentAsync(purchase.AsaasPaymentId, cancellationToken);
            } 
            catch (Exception ex) 
            {
                // Se der erro ao cancelar no Asaas (ex: já estava cancelado lá, ou erro de rede),
                // ainda vamos seguir e cancelar no nosso banco para não travar o usuário.
                Console.WriteLine($"Aviso: Falha ao cancelar no Asaas. {ex.Message}");
            }
        }

        purchase.Cancel();
        await _context.SaveChangesAsync(cancellationToken);

        return new CancelPurchaseResponseDto
        {
            Success = true,
            Message = "Pagamento cancelado com sucesso."
        };
    }
}
