using MediatR;

namespace Nexas.Application.Portal.Forum.Topics.Commands.ReopenForumTopic;

public record ReopenForumTopicCommand(int Id) : IRequest;
