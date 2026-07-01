namespace Nexas.Application.Checkout.Commands;

public class CheckoutPixResponseDto
{
    public bool Sucesso { get; set; }
    public string CobrancaId { get; set; } = string.Empty;
    public string PixCopiaECola { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;
}
