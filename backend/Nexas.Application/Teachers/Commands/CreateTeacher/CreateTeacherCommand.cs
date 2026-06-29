using MediatR;
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

        public CreateTeacherCommandHandler(INexasDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateTeacherCommand request, CancellationToken cancellationToken)
        {
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
