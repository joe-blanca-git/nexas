using MediatR;

namespace Nexas.Application.Portal.Courses.Queries.GetCourseCheckoutSummary;

public record GetCourseCheckoutSummaryQuery(int CourseId) : IRequest<PortalCourseCheckoutSummaryDto?>;
