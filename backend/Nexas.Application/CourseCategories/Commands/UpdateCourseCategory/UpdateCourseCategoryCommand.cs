using MediatR;
using FluentValidation;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

namespace Nexas.Application.CourseCategories.Commands.UpdateCourseCategory;

public record UpdateCourseCategoryCommand(int Id, string Name, string? Description, bool Active) : IRequest<Unit>;

public class UpdateCourseCategoryCommandValidator : AbstractValidator<UpdateCourseCategoryCommand>
{
    public UpdateCourseCategoryCommandValidator()
    {
        RuleFor(v => v.Name)
            .MaximumLength(150).WithMessage("Name must not exceed 150 characters.")
            .NotEmpty().WithMessage("Name is required.");
    }
}

public class UpdateCourseCategoryCommandHandler : IRequestHandler<UpdateCourseCategoryCommand, Unit>
{
    private readonly INexasDbContext _context;

    public UpdateCourseCategoryCommandHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateCourseCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.CourseCategories.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"CourseCategory with ID {request.Id} not found.");
        }

        entity.Update(request.Name, request.Description, request.Active);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
