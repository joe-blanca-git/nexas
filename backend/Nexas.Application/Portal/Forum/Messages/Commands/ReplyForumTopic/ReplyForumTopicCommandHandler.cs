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
        var topicExists = await _context.ForumTopics.AnyAsync(t => t.Id == request.TopicId, cancellationToken);

        if (!topicExists)
            throw new Exception("Tópico não encontrado.");

        var currentUser = await _userContextService.GetCurrentUserAsync();

        var message = ForumMessage.Create(request.TopicId, request.Content, currentUser.Id);

        _context.ForumMessages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        return message.Id;
    }
}
