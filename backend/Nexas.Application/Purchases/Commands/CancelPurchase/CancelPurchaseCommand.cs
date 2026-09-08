using MediatR;

namespace Nexas.Application.Purchases.Commands.CancelPurchase;

public record CancelPurchaseCommand(int PurchaseId) : IRequest<CancelPurchaseResponseDto>;
