using MediatR;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Portal.Forum.Topics.Commands.ReopenForumTopic;

public class ReopenForumTopicCommandHandler : IRequestHandler<ReopenForumTopicCommand>
{
    private readonly INexasDbContext _context;

    public ReopenForumTopicCommandHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReopenForumTopicCommand request, CancellationToken cancellationToken)
    {
        var topic = await _context.ForumTopics.FindAsync(new object[] { request.Id }, cancellationToken);

        if (topic == null)
            throw new Exception("Tópico não encontrado.");

        topic.Reopen();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
