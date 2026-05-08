using MediatR;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Courses.Commands.UpdateCourse
{
    /// <summary>
    /// Comando para atualizar os dados de um curso existente.
    /// </summary>
    public record UpdateCourseCommand : IRequest<Unit>
    {
        /// <summary>ID do curso a ser atualizado.</summary>
        /// <example>1</example>
        public int Id { get; init; }

        /// <summary>Novo nome do curso.</summary>
        /// <example>Desenvolvimento Web com .NET 8 (Atualizado)</example>
        public string Name { get; init; } = string.Empty;

        /// <summary>Nova descrição detalhada do curso.</summary>
        public string? Description { get; init; }

        /// <summary>Novo subtítulo do curso.</summary>
        public string? DescriptionSub { get; init; }

        /// <summary>Novo nível de dificuldade.</summary>
        /// <example>Avançado</example>
        public string? Level { get; init; }

        /// <summary>Novo preço do curso.</summary>
        /// <example>349.90</example>
        public decimal? PriceSingle { get; init; }

        /// <summary>Novo link da imagem de capa do curso.</summary>
        public string? ImgCoverLink { get; init; }

        /// <summary>Novo ID da biblioteca Bunny para o curso.</summary>
        public string? BunnyLibraryId { get; init; }
    }

    public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, Unit>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public UpdateCourseCommandHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<Unit> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();

            var course = await _context.Courses.FindAsync(new object[] { request.Id }, cancellationToken: cancellationToken);
            if (course == null)
                throw new InvalidOperationException($"Curso com ID {request.Id} não encontrado.");

            course.Name = request.Name;
            course.Description = request.Description;
            course.DescriptionSub = request.DescriptionSub;
            course.Level = request.Level;
            course.PriceSingle = request.PriceSingle;
            course.ImgCoverLink = request.ImgCoverLink;
            course.BunnyLibraryId = request.BunnyLibraryId;
            course.UpdatedBy = currentUser.Id;

            _context.Courses.Update(course);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
