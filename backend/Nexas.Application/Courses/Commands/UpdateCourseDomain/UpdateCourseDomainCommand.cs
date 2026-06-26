using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Courses.Commands.UpdateCourseDomain
{
    public class UpdateCourseDomainCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateCourseDomainCommandHandler : IRequestHandler<UpdateCourseDomainCommand, Unit>
    {
        private readonly INexasDbContext _context;

        public UpdateCourseDomainCommandHandler(INexasDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(UpdateCourseDomainCommand request, CancellationToken cancellationToken)
        {
            var domain = await _context.CourseDomains
                .FirstOrDefaultAsync(d => d.Id == request.Id && d.CourseId == request.CourseId, cancellationToken);

            if (domain == null)
            {
                throw new Exception($"CourseDomain with ID {request.Id} not found for Course {request.CourseId}.");
            }

            domain.Title = request.Title;
            domain.Description = request.Description;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
