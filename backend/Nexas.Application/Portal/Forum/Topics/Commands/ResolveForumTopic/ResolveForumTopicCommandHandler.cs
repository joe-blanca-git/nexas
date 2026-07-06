using MediatR;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Portal.Forum.Topics.Commands.ResolveForumTopic;

public class ResolveForumTopicCommandHandler : IRequestHandler<ResolveForumTopicCommand>
{
    private readonly INexasDbContext _context;

    public ResolveForumTopicCommandHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ResolveForumTopicCommand request, CancellationToken cancellationToken)
    {
        var topic = await _context.ForumTopics.FindAsync(new object[] { request.Id }, cancellationToken);

        if (topic == null)
            throw new Exception("Tópico não encontrado.");

        topic.Resolve();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
