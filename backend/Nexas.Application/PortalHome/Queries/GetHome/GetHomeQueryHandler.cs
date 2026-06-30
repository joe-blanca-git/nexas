using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.PortalHome.DTOs;

namespace Nexas.Application.PortalHome.Queries.GetHome;

public class GetHomeQueryHandler : IRequestHandler<GetHomeQuery, PortalHomeDto>
{
    private readonly INexasDbContext _context;

    public GetHomeQueryHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<PortalHomeDto> Handle(GetHomeQuery request, CancellationToken cancellationToken)
    {
        var lastCourse = await _context.Courses
            .AsNoTracking()
            .Where(c => c.Active)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        LatestCourseDto? latestCourseDto = null;
        if (lastCourse != null)
        {
            latestCourseDto = new LatestCourseDto(
                lastCourse.Id,
                lastCourse.Name,
                lastCourse.Description,
                0m, // Rating
                0   // VoteCount
            );
        }

        var latestNews = await _context.BlogPosts
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
            .Take(7)
            .Select(b => new LatestNewsDto(
                b.Id,
                b.HeaderImageUrl,
                b.Tags,
                b.Title,
                b.Subject,
                b.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PortalHomeDto(latestCourseDto, latestNews);
    }
}
