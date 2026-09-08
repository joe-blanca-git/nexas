using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;
using Nexas.Domain.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.Portal.Tickets.Commands.UploadTicketAttachment;

public class UploadTicketAttachmentCommandHandler : IRequestHandler<UploadTicketAttachmentCommand, int>
{
    private readonly INexasDbContext _context;
    private readonly IUserContextService _userContextService;
    private readonly IFileStorageService _fileStorage;

    public UploadTicketAttachmentCommandHandler(INexasDbContext context, IUserContextService userContextService, IFileStorageService fileStorage)
    {
        _context = context;
        _userContextService = userContextService;
        _fileStorage = fileStorage;
    }

    public async Task<int> Handle(UploadTicketAttachmentCommand request, CancellationToken cancellationToken)
    {
        var user = await _userContextService.GetCurrentUserAsync();

        var message = await _context.TicketMessages
            .Include(m => m.Ticket)
            .FirstOrDefaultAsync(m => m.Id == request.MessageId && m.TicketId == request.TicketId && m.Ticket.UserId == user.Id, cancellationToken);

        if (message == null)
            return 0;

        string bucket = "tickets";
        string path = await _fileStorage.UploadAsync(bucket, request.FileName, request.Content, request.ContentType);

        var attachment = TicketAttachment.Create(
            message.Id, 
            request.FileName, 
            path, 
            bucket, 
            path, 
            request.ContentType, 
            request.Content.Length);

        _context.TicketAttachments.Add(attachment);

        var timeline = TicketTimeline.Create(message.TicketId, user.Id, TicketTimelineEvent.AttachmentUploaded, $"Anexo enviado: {request.FileName}");
        _context.TicketTimelines.Add(timeline);

        await _context.SaveChangesAsync(cancellationToken);

        return attachment.Id;
    }
}
