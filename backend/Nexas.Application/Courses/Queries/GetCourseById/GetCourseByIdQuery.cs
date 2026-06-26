using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.Courses.Queries.GetCourses;

namespace Nexas.Application.Courses.Queries.GetCourseById
{
    /// <summary>
    /// Consulta para obter os detalhes de um curso específico pelo ID (incluindo módulos e aulas).
    /// </summary>
    public record GetCourseByIdQuery : IRequest<CourseDto?>
    {
        /// <summary>ID do curso.</summary>
        public int Id { get; init; }
    }

    public class GetCourseByIdQueryHandler : IRequestHandler<GetCourseByIdQuery, CourseDto?>
    {
        private readonly INexasDbContext _context;

        public GetCourseByIdQueryHandler(INexasDbContext context)
        {
            _context = context;
        }

        public async Task<CourseDto?> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
        {
            return await _context.Courses
                .Where(c => c.Id == request.Id && c.Active)
                .Include(c => c.Domains)
                .Include(c => c.Modules.Where(m => m.Active))
                    .ThenInclude(m => m.Lessons.Where(l => l.Active))
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    DescriptionSub = c.DescriptionSub,
                    Level = c.Level,
                    PriceSingle = c.PriceSingle,
                    ImgCoverLink = c.ImgCoverLink,
                    BunnyLibraryId = c.BunnyLibraryId,
                    Modules = c.Modules.Select(m => new ModuleDto
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Description = m.Description,
                        DescriptionSub = m.DescriptionSub,
                        ImgCoverLink = m.ImgCoverLink,
                        BunnyCollectionId = m.BunnyCollectionId,
                        Lessons = m.Lessons.Select(l => new LessonDto
                        {
                            Id = l.Id,
                            Name = l.Name,
                            Description = l.Description,
                            DurationSeconds = l.DurationSeconds,
                            BunnyVideoId = l.BunnyVideoId
                        }).ToList()
                    }).ToList(),
                    Domains = c.Domains.Select(d => new CourseDomainDto
                    {
                        Id = d.Id,
                        Title = d.Title,
                        Description = d.Description
                    }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
