using MediatR;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Teachers.Commands.DeleteTeacher
{
    public record DeleteTeacherCommand : IRequest<bool>
    {
        public int Id { get; init; }
        public int? CurrentUserId { get; init; }
    }

    public class DeleteTeacherCommandHandler : IRequestHandler<DeleteTeacherCommand, bool>
    {
        private readonly INexasDbContext _context;

        public DeleteTeacherCommandHandler(INexasDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteTeacherCommand request, CancellationToken cancellationToken)
        {
            var teacher = await _context.Teachers.FindAsync(new object[] { request.Id }, cancellationToken);

            if (teacher == null || !teacher.Active)
            {
                return false;
            }

            teacher.Active = false; // Soft delete
            teacher.UpdatedBy = request.CurrentUserId;
            teacher.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
