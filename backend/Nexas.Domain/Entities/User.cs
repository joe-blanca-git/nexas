using Nexas.Domain.Common;

namespace Nexas.Domain.Entities;

public class User : BaseEntity
{
    // REMOVIDO: public int UserId { get; private set; } 
    // MOTIVO: O 'Id' herdado de BaseEntity já será mapeado para a coluna UserId do banco.

    public string ExternalId { get; private set; } = null!;
    public string? FullName { get; private set; }
    public string? Email { get; private set; }
    public string? CpfCnpj { get; private set; }
    public string? AsaasCustomerId { get; private set; }

    // Relacionamentos (Necessários para o EF não se perder nas consultas de compra/matrícula)
    public virtual ICollection<Enrollment> Enrollments { get; private set; } = new List<Enrollment>();
    public virtual ICollection<Purchase> Purchases { get; private set; } = new List<Purchase>();
    public virtual ICollection<LessonView> LessonViews { get; set; } = new List<LessonView>();
    public virtual ICollection<CourseRate> CourseRates { get; set; } = new List<CourseRate>();

    private User() { }

    public static User Create(string externalId, string email, string? fullName = null)
    {
        return new User
        {
            ExternalId = externalId,
            Email = email,
            FullName = fullName,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateProfile(string fullName, string? cpfCnpj)
    {
        FullName = fullName;
        CpfCnpj = cpfCnpj;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAsaasCustomerId(string customerId)
    {
        AsaasCustomerId = customerId;
        UpdatedAt = DateTime.UtcNow;
    }
}