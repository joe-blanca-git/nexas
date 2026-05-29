using Nexas.Domain.Common;

namespace Nexas.Domain.Entities;

public class User : BaseEntity
{
    public int UserId { get; private set; }
    public string ExternalId { get; private set; } = null!;
    public string? FullName { get; private set; }
    public string? Email { get; private set; }
    public string? CpfCnpj { get; private set; }
    public string? AsaasCustomerId { get; private set; }

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

    public void UpdateProfile(string fullName, string cpfCnpj)
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