using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

namespace Nexas.Application.Teachers.Commands.CreateTeacher
{
    public record CreateTeacherCommand : IRequest<int>
    {
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

    public class CreateTeacherCommandHandler : IRequestHandler<CreateTeacherCommand, int>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public CreateTeacherCommandHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<int> Handle(CreateTeacherCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();
            var loggedTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.IdAgivys == currentUser.ExternalId, cancellationToken);

            if (loggedTeacher == null || loggedTeacher.Role != "Admin")
            {
                throw new UnauthorizedAccessException("Apenas administradores podem criar professores.");
            }

            var teacher = Teacher.Create(
                request.Name,
                request.Role,
                request.Position,
                request.Avatar,
                request.Bio,
                request.InstagramLink,
                request.LinkedinLink,
                request.IdAgivys,
                request.CurrentUserId
            );

            teacher.CreatedAt = DateTime.UtcNow;

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync(cancellationToken);

            return teacher.Id;
        }
    }
}
