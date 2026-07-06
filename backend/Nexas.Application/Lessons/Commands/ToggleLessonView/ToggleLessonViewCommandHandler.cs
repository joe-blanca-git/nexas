using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

namespace Nexas.Application.Lessons.Commands.ToggleLessonView;

public class ToggleLessonViewCommandHandler : IRequestHandler<ToggleLessonViewCommand, bool>
{
    private readonly INexasDbContext _context;
    private readonly IUserContextService _userContextService;

    public ToggleLessonViewCommandHandler(INexasDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<bool> Handle(ToggleLessonViewCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _userContextService.GetCurrentUserAsync();
        
        var lessonView = await _context.LessonViews
            .FirstOrDefaultAsync(lv => lv.UserId == currentUser.Id && lv.LessonId == request.LessonId, cancellationToken);

        if (lessonView != null)
        {
            _context.LessonViews.Remove(lessonView);
            await _context.SaveChangesAsync(cancellationToken);
            return false;
        }

        var newLessonView = LessonView.Create(currentUser.Id, request.LessonId);
        _context.LessonViews.Add(newLessonView);
        
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
