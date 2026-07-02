namespace Nexas.Application.Common.Interfaces;

public interface IPaymentEventPublisher
{
    Task PublishPaymentConfirmedAsync(string userId, string tipoCompra, int cursoId);
    Task PublishPaymentRefundedAsync(string userId, string tipoCompra, int cursoId);
}
