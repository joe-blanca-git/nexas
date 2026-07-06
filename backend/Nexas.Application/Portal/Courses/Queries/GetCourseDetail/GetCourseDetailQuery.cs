using MediatR;

namespace Nexas.Application.Portal.Courses.Queries.GetCourseDetail;

public record GetCourseDetailQuery(int CourseId) : IRequest<GetCourseDetailResponseDto?>;
