namespace Nexas.Application.Checkout.Commands;

public class CheckoutCardResponseDto
{
    public bool Sucesso { get; set; }
    public int PurchaseId { get; set; }
    public string CobrancaId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
