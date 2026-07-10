using MediatR;

namespace Nexas.Application.Portal.Courses.Commands.RateCourse;

public class RateCourseCommand : IRequest<RateCourseResponseDto>
{
    public int CourseId { get; set; }
    public int Rate { get; set; }
}

public class RateCourseResponseDto
{
    public bool Success { get; set; }
    public int CourseId { get; set; }
    public int UserRate { get; set; }
    public double AverageRate { get; set; }
    public int TotalRatings { get; set; }
}
