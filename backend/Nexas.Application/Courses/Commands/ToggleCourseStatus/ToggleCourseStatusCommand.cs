using MediatR;
using Nexas.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.Courses.Commands.ToggleCourseStatus
{
    public record ToggleCourseStatusCommand(int Id) : IRequest<bool>;

    public class ToggleCourseStatusCommandHandler : IRequestHandler<ToggleCourseStatusCommand, bool>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public ToggleCourseStatusCommandHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<bool> Handle(ToggleCourseStatusCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();

            var course = await _context.Courses.FindAsync(new object[] { request.Id }, cancellationToken: cancellationToken);
            if (course == null)
                throw new InvalidOperationException($"Curso com ID {request.Id} não encontrado.");

            course.Active = !course.Active;
            course.UpdatedBy = currentUser.Id;

            _context.Courses.Update(course);
            await _context.SaveChangesAsync(cancellationToken);

            return course.Active;
        }
    }
}
