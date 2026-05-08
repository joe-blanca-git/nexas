using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Courses.Queries.GetCourses
{
    /// <summary>
    /// Consulta para obter a lista de cursos ativos com todos os detalhes (módulos e aulas).
    /// </summary>
    public record GetCoursesQuery : IRequest<List<CourseDto>>;

    /// <summary>
    /// DTO representando um curso completo com módulos e aulas.
    /// </summary>
    public record CourseDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? DescriptionSub { get; init; }
        public string? Level { get; init; }
        public decimal? PriceSingle { get; init; }
        public string? ImgCoverLink { get; init; }
        public string? BunnyLibraryId { get; init; }
        public List<ModuleDto> Modules { get; init; } = new();
    }

    /// <summary>
    /// DTO representando um módulo com suas aulas.
    /// </summary>
    public record ModuleDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? DescriptionSub { get; init; }
        public string? ImgCoverLink { get; init; }
        public string? BunnyCollectionId { get; init; }
        public List<LessonDto> Lessons { get; init; } = new();
    }

    /// <summary>
    /// DTO representando uma aula.
    /// </summary>
    public record LessonDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public int? DurationSeconds { get; init; }
        public string? BunnyVideoId { get; init; }
    }

    public class GetCoursesQueryHandler : IRequestHandler<GetCoursesQuery, List<CourseDto>>
    {
        private readonly INexasDbContext _context;

        public GetCoursesQueryHandler(INexasDbContext context)
        {
            _context = context;
        }

        public async Task<List<CourseDto>> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
        {
            return await _context.Courses
                .Where(c => c.Active)
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
                    }).ToList()
                })
                .ToListAsync(cancellationToken);
        }
    }
}
