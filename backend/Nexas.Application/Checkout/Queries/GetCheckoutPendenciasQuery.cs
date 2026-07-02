using MediatR;

namespace Nexas.Application.Checkout.Queries;

public class CheckoutPendenciasResponseDto
{
    public bool TemPendencia { get; set; }
    public string? Status { get; set; }
    public string? MetodoPagamento { get; set; }
    public string? PixCopiaECola { get; set; }
    public string? QrCodeBase64 { get; set; }
    public string? Mensagem { get; set; }
    public bool JaPago { get; set; }
}

public class GetCheckoutPendenciasQuery : IRequest<CheckoutPendenciasResponseDto>
{
    public int? CursoId { get; set; }
    public string TipoCompra { get; set; } = null!; // AVULSO ou ANUAL
}
