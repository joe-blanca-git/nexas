using MediatR;
using Nexas.Domain.Enums;

namespace Nexas.Application.Portal.Forum.Topics.Queries.GetForumTopics;

public class GetForumTopicsQuery : IRequest<List<ForumTopicSummaryDto>>
{
    public int? CategoryId { get; set; }
    public int? LessonId { get; set; }
    public ForumTopicStatus? Status { get; set; }
    public string? SearchTitle { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
