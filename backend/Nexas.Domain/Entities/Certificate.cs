using Nexas.Domain.Common;

namespace Nexas.Domain.Entities;

public class Certificate : BaseEntity
{
    public int UserId { get; set; }
    public int CourseId { get; set; }
    public string ValidationCode { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }

    // Navigations
    public virtual User User { get; set; } = null!;
    public virtual Course Course { get; set; } = null!;

    private Certificate() { }

    public static Certificate Create(int userId, int courseId, string validationCode)
    {
        return new Certificate
        {
            UserId = userId,
            CourseId = courseId,
            ValidationCode = validationCode,
            IssuedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }
}
