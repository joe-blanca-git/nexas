using MediatR;

namespace Nexas.Application.Portal.Forum.Topics.Commands.ResolveForumTopic;

public record ResolveForumTopicCommand(int Id) : IRequest;
