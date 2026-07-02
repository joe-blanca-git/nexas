using Nexas.Domain.Common;
using Nexas.Domain.Enums;

namespace Nexas.Domain.Entities;

public class Purchase : BaseEntity
{
    public int UserId { get; private set; }
    public int CourseId { get; private set; }
    public decimal Amount { get; private set; }
    public PurchaseStatus Status { get; private set; }
    public string PaymentMethod { get; private set; } = null!;
    public string? AsaasPaymentId { get; private set; }

    // Propriedades de Navegação
    public virtual User User { get; private set; } = null!;
    public virtual Course Course { get; private set; } = null!;

    private Purchase() { } // Requisito do EF Core

    private Purchase(int userId, int courseId, decimal amount, string paymentMethod)
    {
        UserId = userId;
        CourseId = courseId;
        Amount = amount;
        PaymentMethod = paymentMethod;
        Status = PurchaseStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public static Purchase Create(int userId, int courseId, decimal amount, string paymentMethod)
    {
        if (userId <= 0) throw new ArgumentException("UserId inválido.");
        if (courseId <= 0) throw new ArgumentException("CourseId inválido.");
        if (amount <= 0) throw new ArgumentException("Amount deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(paymentMethod)) throw new ArgumentException("Método de pagamento é obrigatório.");

        return new Purchase(userId, courseId, amount, paymentMethod);
    }

    public void SetUser(User user)
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
    }

    public void SetCourse(Course course)
    {
        Course = course ?? throw new ArgumentNullException(nameof(course));
    }

    public void UpdateAsaasPaymentId(string asaasPaymentId)
    {
        if (string.IsNullOrWhiteSpace(asaasPaymentId))
            throw new ArgumentException("O ID do pagamento Asaas não pode ser vazio.");

        AsaasPaymentId = asaasPaymentId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        Status = PurchaseStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Refund()
    {
        Status = PurchaseStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = PurchaseStatus.Canceled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        Status = PurchaseStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }
}