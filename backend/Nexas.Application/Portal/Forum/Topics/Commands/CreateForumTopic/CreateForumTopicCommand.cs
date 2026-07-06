using MediatR;

namespace Nexas.Application.Portal.Forum.Topics.Commands.CreateForumTopic;

public record CreateForumTopicCommand(int CategoryId, int? LessonId, string Title, string Content) : IRequest<int>;
