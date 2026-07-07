using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Portal.Forum.Topics.Queries.GetForumTopics;

public class GetForumTopicsQueryHandler : IRequestHandler<GetForumTopicsQuery, List<ForumTopicSummaryDto>>
{
    private readonly INexasDbContext _context;

    public GetForumTopicsQueryHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<List<ForumTopicSummaryDto>> Handle(GetForumTopicsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ForumTopics
            .Include(t => t.Category)
            .Include(t => t.Author)
            .Include(t => t.Messages)
            .AsNoTracking();

        if (request.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == request.CategoryId.Value);

        if (request.LessonId.HasValue)
            query = query.Where(t => t.LessonId == request.LessonId.Value);

        if (request.Status.HasValue)
            query = query.Where(t => t.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTitle))
            query = query.Where(t => t.Title.Contains(request.SearchTitle));

        var skip = (request.Page - 1) * request.PageSize;

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(t => new ForumTopicSummaryDto
            {
                Id = t.Id,
                Title = t.Title,
                Subject = t.Subject,
                Status = t.Status.ToString(),
                CategoryName = t.Category.Name,
                AuthorName = t.Author.FullName ?? "Anônimo",
                RepliesCount = t.Messages.Count,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
