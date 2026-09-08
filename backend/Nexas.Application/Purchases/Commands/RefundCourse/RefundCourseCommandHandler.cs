using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;
using Nexas.Domain.Enums;

namespace Nexas.Application.Purchases.Commands.RefundCourse;

public class RefundCourseCommandHandler : IRequestHandler<RefundCourseCommand, RefundCourseResponseDto>
{
    private readonly INexasDbContext _context;
    private readonly IAsaasService _asaasService;
    private readonly IUserContextService _userContextService;

    public RefundCourseCommandHandler(INexasDbContext context, IAsaasService asaasService, IUserContextService userContextService)
    {
        _context = context;
        _asaasService = asaasService;
        _userContextService = userContextService;
    }

    public async Task<RefundCourseResponseDto> Handle(RefundCourseCommand request, CancellationToken cancellationToken)
    {
        var currentUser = await _userContextService.GetCurrentUserAsync();
            var purchase = await _context.Purchases
                .FirstOrDefaultAsync(p => p.Id == request.PurchaseId, cancellationToken);

            if (purchase == null)
            {
                return new RefundCourseResponseDto
                {
                    Success = false,
                    Message = "Compra não encontrada."
                };
            }

            if (purchase.Status != Domain.Enums.PurchaseStatus.Approved)
            {
                return new RefundCourseResponseDto
                {
                    Success = false,
                    Message = "Compra não está aprovada para estorno."
                };
            }

            var deadline = purchase.CreatedAt.AddDays(7);
            if (DateTime.UtcNow > deadline)
            {
                return new RefundCourseResponseDto
                {
                    Success = false,
                    Message = "Prazo de estorno expirado."
                };
            }

            if (string.IsNullOrWhiteSpace(purchase.AsaasPaymentId))
            {
                return new RefundCourseResponseDto
                {
                    Success = false,
                    Message = "Pagamento Asaas não encontrado para esta compra."
                };
            }

            var courseData = await _context.Courses
                .Where(c => c.Id == purchase.CourseId)
                .Select(c => new {
                    TotalLessons = c.Modules.SelectMany(m => m.Lessons).Count(l => l.Active),
                    CompletedLessons = c.Modules.SelectMany(m => m.Lessons).SelectMany(l => l.LessonViews).Count(lv => lv.UserId == purchase.UserId)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (courseData != null)
            {
                double progress = courseData.TotalLessons > 0 ? (courseData.CompletedLessons * 100.0) / courseData.TotalLessons : 0;
                if (progress > 20)
                {
                    return new RefundCourseResponseDto
                    {
                        Success = false,
                        Message = "O estorno não pode ser realizado porque o progresso do curso ultrapassou 20%."
                    };
                }
            }

            purchase.MarkAsRefundRequested();

            var requestCode = $"REF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
            
            // Criação automática de Chamado (Ticket)
            var category = await _context.TicketCategories.FirstOrDefaultAsync(c => c.Active && (c.Description.Contains("Financeiro") || c.Description.Contains("Reembolso")), cancellationToken)
                        ?? await _context.TicketCategories.FirstOrDefaultAsync(c => c.Active, cancellationToken);
            
            var ticket = Ticket.Create(purchase.UserId, category?.Id ?? 1, $"Solicitação de Reembolso - Pedido {requestCode}", TicketPriority.High);
            
            // Agora adicionamos a primeira mensagem e a timeline no mesmo objeto ticket
            var msg = TicketMessage.Create(ticket.Id, purchase.UserId, Nexas.Domain.Enums.TicketOrigin.Portal, "Solicitação de reembolso aberta através do Portal do Aluno.", null);
            ticket.Messages.Add(msg);

            var timeline = TicketTimeline.Create(ticket.Id, purchase.UserId, TicketTimelineEvent.Created, "Ticket de reembolso criado automaticamente.");
            ticket.Timelines.Add(timeline);

            _context.Tickets.Add(ticket);
            
            // Salvamos o ticket para o EF Core gerar e preencher o ticket.Id (auto-increment)
            await _context.SaveChangesAsync(cancellationToken);

            var refundRequest = RefundRequest.Create(purchase.Id, requestCode, "Solicitado pelo aluno no portal", ticket.Id.ToString());
            _context.RefundRequests.Add(refundRequest);
            await _context.SaveChangesAsync(cancellationToken);

            return new RefundCourseResponseDto
            {
                Success = true,
                Message = "Sua solicitação de estorno foi enviada e está em análise."
            };
    }
}
