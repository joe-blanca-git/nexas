using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Courses.Commands.DeleteModule
{
    /// <summary>
    /// Comando para deletar permanentemente um módulo e todas as suas aulas.
    /// </summary>
    public record DeleteModuleCommand : IRequest<Unit>
    {
        /// <summary>ID do módulo a ser deletado.</summary>
        /// <example>1</example>
        public int Id { get; init; }
    }

    public class DeleteModuleCommandHandler : IRequestHandler<DeleteModuleCommand, Unit>
    {
        private readonly INexasDbContext _context;

        public DeleteModuleCommandHandler(INexasDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
        {
            var module = await _context.Modules
                .Include(m => m.Lessons)
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (module == null)
                throw new InvalidOperationException($"Módulo com ID {request.Id} não encontrado.");

            // Remover todas as aulas do módulo
            _context.Lessons.RemoveRange(module.Lessons);

            // Remover o módulo
            _context.Modules.Remove(module);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
