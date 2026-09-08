namespace Nexas.Application.Checkout.Commands;

public class CheckoutCardRequestDto
{
    public int CursoId { get; set; }
    public decimal Valor { get; set; }
    public string TipoCompra { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty; // "CREDIT" or "DEBIT"
    public string HolderName { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string ExpiryMonth { get; set; } = string.Empty;
    public string ExpiryYear { get; set; } = string.Empty;
    public string Ccv { get; set; } = string.Empty;
    public int Installments { get; set; } = 1;
}
