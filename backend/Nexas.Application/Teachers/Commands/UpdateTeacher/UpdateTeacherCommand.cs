using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Teachers.Commands.UpdateTeacher
{
    public record UpdateTeacherCommand : IRequest<bool>
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Role { get; init; }
        public string? Position { get; init; }
        public string? Avatar { get; init; }
        public string? Bio { get; init; }
        public string? InstagramLink { get; init; }
        public string? LinkedinLink { get; init; }
        public string? IdAgivys { get; init; }
        public int? CurrentUserId { get; init; }
    }

    public class UpdateTeacherCommandHandler : IRequestHandler<UpdateTeacherCommand, bool>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public UpdateTeacherCommandHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<bool> Handle(UpdateTeacherCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();
            var loggedTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.IdAgivys == currentUser.ExternalId, cancellationToken);

            if (loggedTeacher == null) return false;

            if (loggedTeacher.Role != "Admin" && loggedTeacher.Id != request.Id)
            {
                return false; // Cannot edit another teacher
            }

            var teacher = await _context.Teachers.FindAsync(new object[] { request.Id }, cancellationToken);

            if (teacher == null || !teacher.Active)
            {
                return false; // Or throw NotFoundException
            }

            teacher.Name = request.Name;
            
            // Only Admin can change roles
            if (loggedTeacher.Role == "Admin")
            {
                teacher.Role = request.Role;
            }

            teacher.Position = request.Position;
            teacher.Avatar = request.Avatar;
            teacher.Bio = request.Bio;
            teacher.InstagramLink = request.InstagramLink;
            teacher.LinkedinLink = request.LinkedinLink;
            teacher.IdAgivys = request.IdAgivys;
            teacher.UpdatedBy = request.CurrentUserId;
            teacher.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
