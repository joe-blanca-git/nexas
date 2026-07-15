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
        
        /// <summary>Indica se deve incluir curso, módulos e aulas inativos (uso em portais admin).</summary>
        public bool IncludeInactive { get; init; } = false;

        /// <summary>Indica se deve filtrar pelo professor atual.</summary>
        public bool FilterByCurrentUserTeacher { get; init; } = false;
    }

    public class GetCourseByIdQueryHandler : IRequestHandler<GetCourseByIdQuery, CourseDto?>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public GetCourseByIdQueryHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<CourseDto?> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Courses.AsQueryable();
            
            if (!request.IncludeInactive)
                query = query.Where(c => c.Active);

            if (request.FilterByCurrentUserTeacher)
            {
                var currentUser = await _userContextService.GetCurrentUserAsync();
                query = query.Where(c => c.CourseTeachers.Any(ct => ct.Teacher.IdAgivys == currentUser.ExternalId));
            }

            return await query
                .Where(c => c.Id == request.Id)
                .Include(c => c.Domains)
                .Include(c => c.CourseTeachers)
                    .ThenInclude(ct => ct.Teacher)
                .Include(c => c.Modules)
                    .ThenInclude(m => m.Lessons)
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
                    Modules = c.Modules.Where(m => request.IncludeInactive || m.Active).Select(m => new ModuleDto
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Description = m.Description,
                        DescriptionSub = m.DescriptionSub,
                        ImgCoverLink = m.ImgCoverLink,
                        BunnyCollectionId = m.BunnyCollectionId,
                        Lessons = m.Lessons.Where(l => request.IncludeInactive || l.Active).Select(l => new LessonDto
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
                    }).ToList(),
                    Teachers = c.CourseTeachers.Where(ct => ct.Teacher.Active).Select(ct => new Nexas.Application.Teachers.Common.TeacherDto
                    {
                        Id = ct.Teacher.Id,
                        Name = ct.Teacher.Name,
                        Role = ct.Teacher.Role,
                        Position = ct.Teacher.Position,
                        Avatar = ct.Teacher.Avatar,
                        Bio = ct.Teacher.Bio,
                        InstagramLink = ct.Teacher.InstagramLink,
                        LinkedinLink = ct.Teacher.LinkedinLink,
                        IdAgivys = ct.Teacher.IdAgivys
                    }).ToList(),
                    Categories = c.CourseCategories.Select(cc => new Nexas.Application.Courses.Common.CourseCategoryBasicDto(cc.Category.Id, cc.Category.Name)).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
