using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.Teachers.Common;

namespace Nexas.Application.Teachers.Queries.GetTeacherById
{
    public record GetTeacherByIdQuery : IRequest<TeacherDto?>
    {
        public int Id { get; init; }
    }

    public class GetTeacherByIdQueryHandler : IRequestHandler<GetTeacherByIdQuery, TeacherDto?>
    {
        private readonly INexasDbContext _context;

        public GetTeacherByIdQueryHandler(INexasDbContext context)
        {
            _context = context;
        }

        public async Task<TeacherDto?> Handle(GetTeacherByIdQuery request, CancellationToken cancellationToken)
        {
            return await _context.Teachers
                .Where(t => t.Id == request.Id && t.Active)
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
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
