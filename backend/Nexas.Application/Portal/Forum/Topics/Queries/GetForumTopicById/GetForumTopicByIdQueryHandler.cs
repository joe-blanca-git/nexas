using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Portal.Forum.Topics.Queries.GetForumTopicById;

public class GetForumTopicByIdQueryHandler : IRequestHandler<GetForumTopicByIdQuery, ForumTopicDetailDto>
{
    private readonly INexasDbContext _context;

    public GetForumTopicByIdQueryHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<ForumTopicDetailDto> Handle(GetForumTopicByIdQuery request, CancellationToken cancellationToken)
    {
        var topic = await _context.ForumTopics
            .Include(t => t.Category)
            .Include(t => t.Author)
            .Include(t => t.Lesson)
            .Include(t => t.Messages)
                .ThenInclude(m => m.Author)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (topic == null)
            throw new Exception("Tópico não encontrado.");

        return new ForumTopicDetailDto
        {
            Id = topic.Id,
            Title = topic.Title,
            Content = topic.Content,
            Status = topic.Status.ToString(),
            CategoryName = topic.Category.Name,
            AuthorName = topic.Author.FullName ?? "Anônimo",
            LessonName = topic.Lesson?.Name,
            CreatedAt = topic.CreatedAt,
            Messages = topic.Messages.Select(m => new ForumMessageDto
            {
                Id = m.Id,
                Content = m.Content,
                AuthorName = m.Author.FullName ?? "Anônimo",
                CreatedAt = m.CreatedAt
            }).ToList()
        };
    }
}
