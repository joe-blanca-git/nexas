using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.Teachers.Common;

namespace Nexas.Application.Teachers.Queries.GetTeacherById
{
    public record GetTeacherByIdQuery : IRequest<TeacherDto?>
    {
        public int Id { get; init; }
        public bool IsPublic { get; init; } = false;
    }

    public class GetTeacherByIdQueryHandler : IRequestHandler<GetTeacherByIdQuery, TeacherDto?>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public GetTeacherByIdQueryHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<TeacherDto?> Handle(GetTeacherByIdQuery request, CancellationToken cancellationToken)
        {
            if (request.IsPublic)
            {
                return await _context.Teachers
                    .Where(t => t.Id == request.Id && t.Active)
                    .Select(t => new TeacherDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Role = t.Role,
                        Position = t.Position,
                        Avatar = t.Avatar,
                        Bio = t.Bio,
                        InstagramLink = t.InstagramLink,
                        LinkedinLink = t.LinkedinLink,
                        IdAgivys = t.IdAgivys
                    })
                    .FirstOrDefaultAsync(cancellationToken);
            }

            var currentUser = await _userContextService.GetCurrentUserAsync();
            var loggedTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.IdAgivys == currentUser.ExternalId, cancellationToken);

            if (loggedTeacher == null)
            {
                return null;
            }

            if (loggedTeacher.Role != "Admin" && loggedTeacher.Id != request.Id)
            {
                return null;
            }

            return await _context.Teachers
                .Where(t => t.Id == request.Id && t.Active)
                .Select(t => new TeacherDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Role = t.Role,
                    Position = t.Position,
                    Avatar = t.Avatar,
                    Bio = t.Bio,
                    InstagramLink = t.InstagramLink,
                    LinkedinLink = t.LinkedinLink,
                    IdAgivys = t.IdAgivys
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
