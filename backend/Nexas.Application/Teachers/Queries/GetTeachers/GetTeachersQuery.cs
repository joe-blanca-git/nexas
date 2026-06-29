using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.Teachers.Common;

namespace Nexas.Application.Teachers.Queries.GetTeachers
{
    public record GetTeachersQuery : IRequest<List<TeacherDto>>;

    public class GetTeachersQueryHandler : IRequestHandler<GetTeachersQuery, List<TeacherDto>>
    {
        private readonly INexasDbContext _context;

        public GetTeachersQueryHandler(INexasDbContext context)
        {
            _context = context;
        }

        public async Task<List<TeacherDto>> Handle(GetTeachersQuery request, CancellationToken cancellationToken)
        {
            return await _context.Teachers
                .Where(t => t.Active)
                .Select(t => new TeacherDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Role = t.Role,
                    Bio = t.Bio,
                    InstagramLink = t.InstagramLink,
                    LinkedinLink = t.LinkedinLink,
                    IdAgivys = t.IdAgivys
                })
                .ToListAsync(cancellationToken);
        }
    }
}
