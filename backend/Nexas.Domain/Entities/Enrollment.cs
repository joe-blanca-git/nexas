using Nexas.Domain.Common;
using Nexas.Domain.Enums;

namespace Nexas.Domain.Entities;

public class Enrollment : BaseEntity
{
    public int UserId { get; private set; }
    public int CourseId { get; private set; }
    public EnrollmentOrigin Origin { get; private set; }
    public bool Active { get; private set; }
    public int? SubscriptionId { get; private set; }

    // Propriedades de Navegação
    public virtual User User { get; private set; } = null!;
    public virtual Course Course { get; private set; } = null!;
    public virtual Subscription? Subscription { get; private set; }

    private Enrollment() { } // Requisito do EF Core

    private Enrollment(int userId, int courseId, EnrollmentOrigin origin, int? subscriptionId = null)
    {
        UserId = userId;
        CourseId = courseId;
        Origin = origin;
        SubscriptionId = subscriptionId;
        Active = true;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory Method para criação de matrícula seguindo o schema SQL.
    /// </summary>
    public static Enrollment Create(int userId, int courseId, EnrollmentOrigin origin, int? subscriptionId = null)
    {
        if (userId <= 0) throw new ArgumentException("UserId inválido.");
        if (courseId <= 0) throw new ArgumentException("CourseId inválido.");

        return new Enrollment(userId, courseId, origin, subscriptionId);
    }

    public void Deactivate() => Active = false;
}