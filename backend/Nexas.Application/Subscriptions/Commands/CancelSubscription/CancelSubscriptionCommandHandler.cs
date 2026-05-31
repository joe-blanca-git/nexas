using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Subscriptions.Commands.CancelSubscription;

public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, CancelSubscriptionResponseDto>
{
    private readonly INexasDbContext _context;
    private readonly IAsaasService _asaasService;
    private readonly IUserContextService _userContextService;

    public CancelSubscriptionCommandHandler(INexasDbContext context, IAsaasService asaasService, IUserContextService userContextService)
    {
        _context = context;
        _asaasService = asaasService;
        _userContextService = userContextService;
    }

    public async Task<CancelSubscriptionResponseDto> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _userContextService.GetCurrentUserAsync();
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId && s.UserId == currentUser.Id, cancellationToken);

        if (subscription == null)
        {
            return new CancelSubscriptionResponseDto
            {
                Success = false,
                Message = "Assinatura não encontrada."
            };
        }

        if (!subscription.StartDate.HasValue)
        {
            return new CancelSubscriptionResponseDto
            {
                Success = false,
                Message = "Assinatura não possui StartDate."
            };
        }

        var deadline = subscription.StartDate.Value.AddDays(7);
        if (DateTime.UtcNow > deadline)
        {
            return new CancelSubscriptionResponseDto
            {
                Success = false,
                Message = "Prazo de cancelamento expirado."
            };
        }

        if (string.IsNullOrWhiteSpace(subscription.AsaasSubscriptionId))
        {
            return new CancelSubscriptionResponseDto
            {
                Success = false,
                Message = "Identificador Asaas não encontrado para esta assinatura."
            };
        }

        await _asaasService.CancelSubscriptionAsync(subscription.AsaasSubscriptionId, cancellationToken);

        subscription.Deactivate();
        subscription.SetEndDate(DateTime.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);

        return new CancelSubscriptionResponseDto
        {
            Success = true,
            Message = "Assinatura cancelada com sucesso."
        };
    }
}
