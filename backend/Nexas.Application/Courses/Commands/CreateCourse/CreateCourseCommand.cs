using MediatR;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

namespace Nexas.Application.Courses.Commands.CreateCourse
{
    /// <summary>
    /// Comando para criação de um novo curso completo com módulos e aulas.
    /// </summary>
    public record CreateCourseCommand : IRequest<int>
    {
        /// <summary>Nome do curso.</summary>
        /// <example>Desenvolvimento Web com .NET 8</example>
        public string Name { get; init; } = string.Empty;

        /// <summary>Descrição detalhada do curso.</summary>
        /// <example>Um curso completo sobre as tecnologias mais modernas do ecossistema .NET.</example>
        public string? Description { get; init; }

        /// <summary>Sub-descrição ou subtítulo do curso.</summary>
        /// <example>Do zero ao avançado em Clean Architecture.</example>
        public string? DescriptionSub { get; init; }

        /// <summary>Nível de dificuldade.</summary>
        /// <example>Avançado</example>
        public string? Level { get; init; }

        /// <summary>Preço de venda do curso individual.</summary>
        /// <example>299.90</example>
        public decimal? PriceSingle { get; init; }

        /// <summary>Link da imagem de capa do curso.</summary>
        /// <example>https://cdn.example.com/covers/course-cover.jpg</example>
        public string? ImgCoverLink { get; init; }

        /// <summary>ID da biblioteca Bunny para o curso.</summary>
        /// <example>library_12345</example>
        public string? BunnyLibraryId { get; init; }

        /// <summary>Lista de módulos que compõem o curso.</summary>
        public List<CreateModuleDto> Modules { get; init; } = new();
    }

    /// <summary>DTO para criação de um módulo.</summary>
    public record CreateModuleDto
    {
        /// <summary>Nome do módulo.</summary>
        /// <example>Módulo 1: Fundamentos</example>
        public string Name { get; init; } = string.Empty;

        /// <summary>Descrição do módulo.</summary>
        public string? Description { get; init; }

        /// <summary>Sub-descrição do módulo.</summary>
        public string? DescriptionSub { get; init; }

        /// <summary>Link da imagem de capa do módulo.</summary>
        /// <example>https://cdn.example.com/covers/module-cover.jpg</example>
        public string? ImgCoverLink { get; init; }

        /// <summary>ID da coleção Bunny para o módulo.</summary>
        /// <example>collection_12345</example>
        public string? BunnyCollectionId { get; init; }

        /// <summary>Lista de aulas do módulo.</summary>
        public List<CreateLessonDto> Lessons { get; init; } = new();
    }

    /// <summary>DTO para criação de uma aula.</summary>
    public record CreateLessonDto
    {
        /// <summary>Nome da aula.</summary>
        /// <example>Aula 1: Introdução ao C#</example>
        public string Name { get; init; } = string.Empty;

        /// <summary>Descrição do conteúdo da aula.</summary>
        public string? Description { get; init; }

        /// <summary>Duração estimada em segundos.</summary>
        /// <example>600</example>
        public int? DurationSeconds { get; init; }

        /// <summary>ID do vídeo Bunny para a aula.</summary>
        /// <example>video_12345</example>
        public string? BunnyVideoId { get; init; }
    }

    public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, int>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public CreateCourseCommandHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<int> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();

            var course = Course.Create(
                request.Name,
                request.Description,
                request.DescriptionSub,
                request.Level,
                request.PriceSingle,
                request.ImgCoverLink,
                request.BunnyLibraryId,
                currentUser.Id
            );

            foreach (var moduleDto in request.Modules)
            {
                var module = Module.Create(
                    moduleDto.Name,
                    moduleDto.Description,
                    moduleDto.DescriptionSub,
                    moduleDto.ImgCoverLink,
                    moduleDto.BunnyCollectionId,
                    currentUser.Id
                );

                foreach (var lessonDto in moduleDto.Lessons)
                {
                    var lesson = Lesson.Create(
                        lessonDto.Name,
                        lessonDto.Description,
                        lessonDto.DurationSeconds,
                        lessonDto.BunnyVideoId,
                        currentUser.Id
                    );

                    module.Lessons.Add(lesson);
                }

                course.Modules.Add(module);
            }

            _context.Courses.Add(course);
            await _context.SaveChangesAsync(cancellationToken);

            return course.Id;
        }
    }
}
