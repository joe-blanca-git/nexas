namespace Nexas.Application.SupportAccess.Queries.GetUserAccessDetails;

public class SupportCourseAccessDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AccessStatus { get; set; } = "Bloqueado"; // Bloqueado ou Liberado
    public decimal? PurchaseValue { get; set; }
    public string? PaymentMethod { get; set; }
    public bool IsFree { get; set; }
}
