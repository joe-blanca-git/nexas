using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.Portal.Tickets.DTOs;
using Nexas.Application.Tickets.DTOs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nexas.Application.Tickets.Queries.GetAdminTicketDetails;

public class GetAdminTicketDetailsQueryHandler : IRequestHandler<GetAdminTicketDetailsQuery, TicketAdminDetailsDto?>
{
    private readonly INexasDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public GetAdminTicketDetailsQueryHandler(INexasDbContext context, IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    public async Task<TicketAdminDetailsDto?> Handle(GetAdminTicketDetailsQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.User)
            .Include(t => t.Timelines)
            .Include(t => t.Messages)
                .ThenInclude(m => m.Attachments)
            .Include(t => t.Messages)
                .ThenInclude(m => m.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (ticket == null) return null;

        var dto = new TicketAdminDetailsDto
        {
            Id = ticket.Id,
            Subject = ticket.Subject,
            Status = ((int)ticket.Status).ToString(),
            Priority = ticket.Priority.ToString(),
            CategoryName = ticket.Category.Description,
            CategoryId = ticket.Category.Id,
            StudentName = ticket.User.FullName ?? "Aluno",
            StudentEmail = ticket.User.Email ?? "",
            CreatedAt = ticket.CreatedAt,
            ClosedAt = ticket.ClosedAt,
            Timeline = ticket.Timelines.OrderBy(tl => tl.CreatedAt).Select(tl => new TicketTimelineDto
            {
                Id = tl.Id,
                Event = tl.Event.ToString(),
                Description = tl.Description,
                CreatedAt = tl.CreatedAt
            }).ToList(),
            Messages = new System.Collections.Generic.List<TicketMessageDto>()
        };

        foreach (var msg in ticket.Messages.OrderBy(m => m.CreatedAt))
        {
            var msgDto = new TicketMessageDto
            {
                Id = msg.Id,
                Origin = msg.Origin.ToString(),
                SenderName = msg.User?.FullName ?? "Usuário",
                Content = msg.Content,
                CreatedAt = msg.CreatedAt
            };

            foreach (var att in msg.Attachments)
            {
                msgDto.Attachments.Add(new TicketAttachmentDto
                {
                    Id = att.Id,
                    OriginalFileName = att.OriginalFileName,
                    Url = _fileStorageService.GenerateTemporaryUrlAsync(att.Bucket, att.Path, 60).GetAwaiter().GetResult()
                });
            }

            dto.Messages.Add(msgDto);
        }

        return dto;
    }
}
