using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.Teachers.Common;

namespace Nexas.Application.Teachers.Queries.GetTeachers
{
    public record GetTeachersQuery(bool IsPublic = false) : IRequest<List<TeacherDto>>;

    public class GetTeachersQueryHandler : IRequestHandler<GetTeachersQuery, List<TeacherDto>>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public GetTeachersQueryHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<List<TeacherDto>> Handle(GetTeachersQuery request, CancellationToken cancellationToken)
        {
            if (request.IsPublic)
            {
                return await _context.Teachers
                    .Where(t => t.Active)
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
                    .ToListAsync(cancellationToken);
            }

            var currentUser = await _userContextService.GetCurrentUserAsync();
            var loggedTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.IdAgivys == currentUser.ExternalId, cancellationToken);

            if (loggedTeacher == null)
            {
                return new List<TeacherDto>();
            }

            var query = _context.Teachers.Where(t => t.Active);


            return await query
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
                .ToListAsync(cancellationToken);
        }
    }
}
