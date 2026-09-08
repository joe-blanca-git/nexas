using MediatR;
using Nexas.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Nexas.Application.Courses.Commands.DeactivateModule
{
    /// <summary>
    /// Comando para inativar um módulo sem deletá-lo do banco de dados.
    /// </summary>
    public record DeactivateModuleCommand : IRequest<Unit>
    {
        /// <summary>ID do módulo a ser inativado.</summary>
        /// <example>1</example>
        public int Id { get; init; }
    }

    public class DeactivateModuleCommandHandler : IRequestHandler<DeactivateModuleCommand, Unit>
    {
        private readonly INexasDbContext _context;
        private readonly IUserContextService _userContextService;

        public DeactivateModuleCommandHandler(INexasDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        public async Task<Unit> Handle(DeactivateModuleCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _userContextService.GetCurrentUserAsync();

            var module = await _context.Modules
                .Include(m => m.Course)
                .ThenInclude(c => c.CourseTeachers)
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
                
            if (module == null)
                throw new InvalidOperationException($"Módulo com ID {request.Id} não encontrado.");

            var currentTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.IdAgivys == currentUser.ExternalId, cancellationToken);
            if (currentTeacher == null || (currentTeacher.Role != "Admin" && !module.Course.CourseTeachers.Any(ct => ct.TeacherId == currentTeacher.Id)))
            {
                throw new UnauthorizedAccessException("Você não tem permissão para inativar este módulo.");
            }

            module.Active = false;
            module.UpdatedBy = currentUser.Id;

            _context.Modules.Update(module);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
