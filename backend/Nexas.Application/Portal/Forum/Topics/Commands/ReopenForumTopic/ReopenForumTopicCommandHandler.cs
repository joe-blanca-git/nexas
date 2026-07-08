using MediatR;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Portal.Forum.Topics.Commands.ReopenForumTopic;

public class ReopenForumTopicCommandHandler : IRequestHandler<ReopenForumTopicCommand>
{
    private readonly INexasDbContext _context;
    private readonly IUserContextService _userContextService;

    public ReopenForumTopicCommandHandler(INexasDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task Handle(ReopenForumTopicCommand request, CancellationToken cancellationToken)
    {
        var topic = await _context.ForumTopics.FindAsync(new object[] { request.Id }, cancellationToken);

        if (topic == null)
            throw new Exception("Tópico não encontrado.");

        var currentUser = await _userContextService.GetCurrentUserAsync();
        if (topic.AuthorId != currentUser.Id)
            throw new UnauthorizedAccessException("Somente o autor do tópico pode reabri-lo.");

        topic.Reopen();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
