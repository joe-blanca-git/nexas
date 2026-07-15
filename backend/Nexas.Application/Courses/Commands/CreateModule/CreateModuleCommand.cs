using MediatR;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Nexas.Application.Courses.Commands.CreateModule
{
    /// <summary>
    /// Comando para criação de um novo módulo em um curso existente.
    /// </summary>
    public record CreateModuleCommand : IRequest<int>
    {
        /// <summary>ID do curso ao qual o módulo será adicionado.</summary>
        /// <example>1</example>
        public int CourseId { get; init; }

        /// <summary>Nome do módulo.</summary>
        /// <example>Módulo 1: Fundamentos</example>
        public string Name { get; init; } = string.Empty;

        /// <summary>Descrição detalhada do módulo.</summary>
        /// <example>Aprenda os fundamentos básicos do framework.</example>
        public string? Description { get; init; }

        /// <summary>Sub-descrição ou subtítulo do módulo.</summary>
        /// <example>Conceitos essenciais</example>
        public string? DescriptionSub { get; init; }

        /// <summary>Link da imagem de capa do módulo.</summary>
        /// <example>https://cdn.example.com/covers/module-cover.jpg</example>
        public string? ImgCoverLink { get; init; }

        /// <summary>ID da coleção Bunny para o módulo.</summary>
        /// <example>collection_12345</example>
        public string? BunnyCollectionId { get; init; }
    }

    public class CreateModuleCommandHandler : IRequestHandler<CreateModuleCommand, int>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public CreateModuleCommandHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<int> Handle(CreateModuleCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();

            // Fetch course with CourseTeachers
            var course = await _context.Courses
                .Include(c => c.CourseTeachers)
                .ThenInclude(ct => ct.Teacher)
                .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);
                
            if (course == null)
                throw new InvalidOperationException($"Curso com ID {request.CourseId} não encontrado.");

            // Check if current user is a teacher of this course
            bool isTeacherOfCourse = course.CourseTeachers.Any(ct => ct.Teacher.IdAgivys == currentUser.ExternalId);
            if (!isTeacherOfCourse)
                throw new UnauthorizedAccessException("Você não tem permissão para adicionar módulos a este curso.");

            var module = Module.Create(
                request.Name,
                request.Description,
                request.DescriptionSub,
                request.ImgCoverLink,
                request.BunnyCollectionId,
                currentUser.Id
            );

            module.CourseId = request.CourseId;

            _context.Modules.Add(module);
            await _context.SaveChangesAsync(cancellationToken);

            return module.Id;
        }
    }
}
