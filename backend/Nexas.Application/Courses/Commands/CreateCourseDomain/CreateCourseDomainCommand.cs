using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

namespace Nexas.Application.Courses.Commands.CreateCourseDomain
{
    public class CreateCourseDomainCommand : IRequest<int>
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class CreateCourseDomainCommandHandler : IRequestHandler<CreateCourseDomainCommand, int>
    {
        private readonly INexasDbContext _context;

        public CreateCourseDomainCommandHandler(INexasDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateCourseDomainCommand request, CancellationToken cancellationToken)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == request.CourseId && c.Active, cancellationToken);

            if (course == null)
            {
                throw new Exception($"Course with ID {request.CourseId} not found.");
            }

            var courseDomain = CourseDomain.Create(request.CourseId, request.Title, request.Description);

            _context.CourseDomains.Add(courseDomain);
            await _context.SaveChangesAsync(cancellationToken);

            return courseDomain.Id;
        }
    }
}
