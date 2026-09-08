using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

namespace Nexas.Application.Portal.Forum.Messages.Commands.ReplyForumTopic;

public class ReplyForumTopicCommandHandler : IRequestHandler<ReplyForumTopicCommand, int>
{
    private readonly INexasDbContext _context;
    private readonly IUserContextService _userContextService;

    public ReplyForumTopicCommandHandler(INexasDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<int> Handle(ReplyForumTopicCommand request, CancellationToken cancellationToken)
    {
        var topic = await _context.ForumTopics.FirstOrDefaultAsync(t => t.Id == request.TopicId, cancellationToken);

        if (topic == null)
            throw new Nexas.Application.Common.Exceptions.NotFoundException("Tópico não encontrado.");

        var currentUser = await _userContextService.GetCurrentUserAsync();

        var message = ForumMessage.Create(request.TopicId, request.Content, currentUser.Id);

        _context.ForumMessages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        return message.Id;
    }
}
