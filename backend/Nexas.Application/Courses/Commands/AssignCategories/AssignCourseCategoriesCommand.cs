using MediatR;
using FluentValidation;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Nexas.Application.Courses.Commands.AssignCategories;

public record AssignCourseCategoriesCommand(int CourseId, List<int> CategoryIds) : IRequest<Unit>;

public class AssignCourseCategoriesCommandValidator : AbstractValidator<AssignCourseCategoriesCommand>
{
    public AssignCourseCategoriesCommandValidator()
    {
        RuleFor(v => v.CourseId).GreaterThan(0).WithMessage("CourseId is required.");
        RuleFor(v => v.CategoryIds).NotNull().WithMessage("CategoryIds cannot be null.");
    }
}

public class AssignCourseCategoriesCommandHandler : IRequestHandler<AssignCourseCategoriesCommand, Unit>
{
    private readonly INexasDbContext _context;

    public AssignCourseCategoriesCommandHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(AssignCourseCategoriesCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .Include(c => c.CourseCategories)
            .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);

        if (course == null)
        {
            throw new InvalidOperationException($"Course with ID {request.CourseId} not found.");
        }

        // Remove old relations
        _context.CourseCourseCategories.RemoveRange(course.CourseCategories);
        
        // Add new relations
        var distinctCategoryIds = request.CategoryIds.Distinct().ToList();
        foreach (var catId in distinctCategoryIds)
        {
            course.CourseCategories.Add(CourseCourseCategory.Create(request.CourseId, catId));
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
