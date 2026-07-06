namespace Nexas.Application.Portal.Financial.Queries.GetMyTransactions;

public class GetMyTransactionsResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
}
