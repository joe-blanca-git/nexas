using MediatR;

namespace Nexas.Application.Portal.Forum.Messages.Commands.ReplyForumTopic;

public record ReplyForumTopicCommand(int TopicId, string Content) : IRequest<int>;
