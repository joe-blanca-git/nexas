using Nexas.Domain.Common;

namespace Nexas.Domain.Entities;

public class LessonView : BaseEntity
{
    public int UserId { get; private set; }
    public int LessonId { get; private set; }

    public virtual User User { get; private set; } = null!;
    public virtual Lesson Lesson { get; private set; } = null!;

    private LessonView() { }

    public static LessonView Create(int userId, int lessonId)
    {
        return new LessonView
        {
            UserId = userId,
            LessonId = lessonId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
