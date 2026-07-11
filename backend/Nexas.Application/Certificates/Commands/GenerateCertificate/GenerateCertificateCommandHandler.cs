using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

namespace Nexas.Application.Certificates.Commands.GenerateCertificate;

public class GenerateCertificateCommandHandler : IRequestHandler<GenerateCertificateCommand, string>
{
    private readonly INexasDbContext _context;
    private readonly IUserContextService _userContextService;

    public GenerateCertificateCommandHandler(INexasDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<string> Handle(GenerateCertificateCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _userContextService.GetCurrentUserAsync();

        // 1. Verify if certificate already exists
        var existingCertificate = await _context.Certificates
            .FirstOrDefaultAsync(c => c.CourseId == request.CourseId && c.UserId == currentUser.Id, cancellationToken);

        if (existingCertificate != null)
        {
            return existingCertificate.ValidationCode;
        }

        // 2. Fetch the course and count total active lessons
        var course = await _context.Courses
            .Include(c => c.Modules)
                .ThenInclude(m => m.Lessons)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CourseId && c.Active, cancellationToken);

        if (course == null)
        {
            throw new InvalidOperationException("Curso não encontrado ou inativo.");
        }

        var courseLessonIds = course.Modules
            .SelectMany(m => m.Lessons)
            .Where(l => l.Active)
            .Select(l => l.Id)
            .ToList();

        var totalLessons = courseLessonIds.Count;

        if (totalLessons == 0)
        {
            throw new InvalidOperationException("O curso não possui aulas.");
        }

        // 3. Count User LessonViews
        var completedLessonsCount = await _context.LessonViews
            .CountAsync(lv => lv.UserId == currentUser.Id && courseLessonIds.Contains(lv.LessonId), cancellationToken);

        // 4. Verify 100% completion
        if (completedLessonsCount < totalLessons)
        {
            throw new InvalidOperationException("O curso não foi concluído a 100%.");
        }

        // 5. Generate Validation Code and save Certificate
        string validationCode = Guid.NewGuid().ToString("N").ToUpper();

        var newCertificate = Certificate.Create(currentUser.Id, request.CourseId, validationCode);
        
        _context.Certificates.Add(newCertificate);
        await _context.SaveChangesAsync(cancellationToken);

        return validationCode;
    }
}
