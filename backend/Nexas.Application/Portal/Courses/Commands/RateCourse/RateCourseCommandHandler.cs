using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

namespace Nexas.Application.Portal.Courses.Commands.RateCourse;

public class RateCourseCommandHandler : IRequestHandler<RateCourseCommand, RateCourseResponseDto>
{
    private readonly INexasDbContext _context;
    private readonly IUserContextService _userContextService;

    public RateCourseCommandHandler(INexasDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<RateCourseResponseDto> Handle(RateCourseCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _userContextService.GetCurrentUserAsync();

        var courseExists = await _context.Courses.AnyAsync(c => c.Id == request.CourseId && c.Active, cancellationToken);
        if (!courseExists)
        {
            return new RateCourseResponseDto { Success = false };
        }

        var courseRate = await _context.CourseRates
            .FirstOrDefaultAsync(cr => cr.CourseId == request.CourseId && cr.UserId == currentUser.Id, cancellationToken);

        if (courseRate == null)
        {
            courseRate = CourseRate.Create(request.CourseId, currentUser.Id, request.Rate);
            _context.CourseRates.Add(courseRate);
        }
        else
        {
            courseRate.UpdateRate(request.Rate);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Fetch updated stats
        var ratings = await _context.CourseRates
            .Where(cr => cr.CourseId == request.CourseId)
            .Select(cr => cr.Rate)
            .ToListAsync(cancellationToken);

        var totalRatings = ratings.Count;
        var averageRate = ratings.Any() ? ratings.Average() : 0;

        return new RateCourseResponseDto
        {
            Success = true,
            CourseId = request.CourseId,
            UserRate = courseRate.Rate,
            AverageRate = Math.Round(averageRate, 1),
            TotalRatings = totalRatings
        };
    }
}
