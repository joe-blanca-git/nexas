using MediatR;
using System.Collections.Generic;

namespace Nexas.Application.Portal.Courses.Queries.GetMyCourses;

public record GetMyCoursesQuery() : IRequest<List<PortalMyCourseDto>>;
