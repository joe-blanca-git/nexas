namespace Nexas.Application.Checkout.Commands;

public class CheckoutPixRequestDto
{
    public int CursoId { get; set; }
    public string TipoCompra { get; set; } = string.Empty; // "AVULSO" ou "ANUAL"
    public string Cpf { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
