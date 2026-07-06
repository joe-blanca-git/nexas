using MediatR;

namespace Nexas.Application.Portal.Forum.Topics.Queries.GetForumTopicById;

public record GetForumTopicByIdQuery(int Id) : IRequest<ForumTopicDetailDto>;
