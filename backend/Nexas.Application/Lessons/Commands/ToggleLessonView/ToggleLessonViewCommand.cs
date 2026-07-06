using MediatR;

namespace Nexas.Application.Lessons.Commands.ToggleLessonView;

public record ToggleLessonViewCommand(int LessonId) : IRequest<bool>;
