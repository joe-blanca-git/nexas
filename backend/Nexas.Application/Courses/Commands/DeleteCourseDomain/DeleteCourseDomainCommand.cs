using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Courses.Commands.DeleteCourseDomain
{
    public class DeleteCourseDomainCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
    }

    public class DeleteCourseDomainCommandHandler : IRequestHandler<DeleteCourseDomainCommand, Unit>
    {
        private readonly INexasDbContext _context;

        public DeleteCourseDomainCommandHandler(INexasDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(DeleteCourseDomainCommand request, CancellationToken cancellationToken)
        {
            var domain = await _context.CourseDomains
                .FirstOrDefaultAsync(d => d.Id == request.Id && d.CourseId == request.CourseId, cancellationToken);

            if (domain == null)
            {
                throw new Exception($"CourseDomain with ID {request.Id} not found for Course {request.CourseId}.");
            }

            _context.CourseDomains.Remove(domain);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
