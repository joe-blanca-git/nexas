using MediatR;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Courses.Commands.UpdateModule
{
    /// <summary>
    /// Comando para atualizar os dados de um módulo existente.
    /// </summary>
    public record UpdateModuleCommand : IRequest<Unit>
    {
        /// <summary>ID do módulo a ser atualizado.</summary>
        /// <example>1</example>
        public int Id { get; init; }

        /// <summary>Novo nome do módulo.</summary>
        /// <example>Módulo 1: Fundamentos (Atualizado)</example>
        public string Name { get; init; } = string.Empty;

        /// <summary>Nova descrição do módulo.</summary>
        public string? Description { get; init; }

        /// <summary>Nova sub-descrição do módulo.</summary>
        public string? DescriptionSub { get; init; }

        /// <summary>Novo link da imagem de capa do módulo.</summary>
        public string? ImgCoverLink { get; init; }

        /// <summary>Novo ID da coleção Bunny para o módulo.</summary>
        public string? BunnyCollectionId { get; init; }
    }

    public class UpdateModuleCommandHandler : IRequestHandler<UpdateModuleCommand, Unit>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public UpdateModuleCommandHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<Unit> Handle(UpdateModuleCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();

            var module = await _context.Modules.FindAsync(new object[] { request.Id }, cancellationToken: cancellationToken);
            if (module == null)
                throw new InvalidOperationException($"Módulo com ID {request.Id} não encontrado.");

            module.Name = request.Name;
            module.Description = request.Description;
            module.DescriptionSub = request.DescriptionSub;
            module.ImgCoverLink = request.ImgCoverLink;
            module.BunnyCollectionId = request.BunnyCollectionId;
            module.UpdatedBy = currentUser.Id;

            _context.Modules.Update(module);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
