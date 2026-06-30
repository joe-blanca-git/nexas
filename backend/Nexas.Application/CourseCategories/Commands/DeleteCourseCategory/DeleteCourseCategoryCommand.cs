using MediatR;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

namespace Nexas.Application.CourseCategories.Commands.DeleteCourseCategory;

public record DeleteCourseCategoryCommand(int Id) : IRequest<Unit>;

public class DeleteCourseCategoryCommandHandler : IRequestHandler<DeleteCourseCategoryCommand, Unit>
{
    private readonly INexasDbContext _context;

    public DeleteCourseCategoryCommandHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteCourseCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.CourseCategories.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"CourseCategory with ID {request.Id} not found.");
        }

        _context.CourseCategories.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
