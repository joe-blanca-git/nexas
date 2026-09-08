using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;
using Nexas.Domain.Enums;

namespace Nexas.Application.Tickets.Commands.ProcessResendWebhook;

public class ProcessResendWebhookCommandHandler : IRequestHandler<ProcessResendWebhookCommand, bool>
{
    private readonly INexasDbContext _context;
    private readonly IEmailService _emailService;

    public ProcessResendWebhookCommandHandler(INexasDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<bool> Handle(ProcessResendWebhookCommand request, CancellationToken cancellationToken)
    {
        // 1. Extrair e-mail
        var from = request.Data.From ?? "";
        var email = ExtractEmail(from);

        if (string.IsNullOrWhiteSpace(email)) return true;

        // 2. Verificar usuário
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (user == null)
        {
            await _emailService.SendTicketUnregisteredUserAsync(email);
            return true;
        }

        // 3. Verificar se é uma resposta
        var subject = request.Data.Subject ?? "";
        var match = Regex.Match(subject, @"\[Ticket\s*#(\d+)\]", RegexOptions.IgnoreCase);
        
        var bodyContent = string.IsNullOrWhiteSpace(request.Data.Text) 
            ? (string.IsNullOrWhiteSpace(request.Data.Html) ? "Sem conteúdo no corpo." : request.Data.Html) 
            : request.Data.Text;

        if (match.Success && int.TryParse(match.Groups[1].Value, out int ticketId))
        {
            var ticket = await _context.Tickets
                .Include(t => t.Messages)
                .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);

            // Verifica se o ticket pertence a este usuário (aluno respondendo)
            // NOTA: Se for o admin respondendo, precisaríamos checar papéis, mas o e-mail inbound
            // geralmente é do aluno para o sistema de suporte.
            if (ticket != null && ticket.UserId == user.Id)
            {
                var message = TicketMessage.Create(ticket.Id, user.Id, TicketOrigin.Email, bodyContent, null);
                _context.TicketMessages.Add(message);

                ticket.UpdateStatus(TicketStatus.Pending); // Volta para Pendente
                ticket.UpdateLastReply();

                var timeline = TicketTimeline.Create(ticket.Id, user.Id, TicketTimelineEvent.Replied, "O aluno respondeu via e-mail.");
                _context.TicketTimelines.Add(timeline);

                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
        }

        // 4. Se não for resposta (ou ticket não for dele), criar novo
        var triagemCategory = await _context.TicketCategories.FirstOrDefaultAsync(c => c.Description == "Triagem", cancellationToken)
                              ?? await _context.TicketCategories.FirstOrDefaultAsync(cancellationToken);

        if (triagemCategory == null) return true; // Falha de consistência do banco

        var newTicket = Ticket.Create(user.Id, triagemCategory.Id, string.IsNullOrWhiteSpace(subject) ? "Sem Assunto" : subject, TicketPriority.Normal);
        _context.Tickets.Add(newTicket);
        await _context.SaveChangesAsync(cancellationToken); // Salva para gerar ID

        var firstMessage = TicketMessage.Create(newTicket.Id, user.Id, TicketOrigin.Email, bodyContent, null);
        _context.TicketMessages.Add(firstMessage);

        var newTimeline = TicketTimeline.Create(newTicket.Id, user.Id, TicketTimelineEvent.Created, "Chamado aberto automaticamente através de e-mail inbound (Resend).");
        _context.TicketTimelines.Add(newTimeline);

        await _context.SaveChangesAsync(cancellationToken);

        await _emailService.SendTicketCreatedAsync(newTicket.Id, user.Email ?? "", user.FullName ?? user.Email ?? "", newTicket.Subject);

        return true;
    }

    private string ExtractEmail(string fromField)
    {
        // Extrai o e-mail de "Nome <email@teste.com>"
        var match = Regex.Match(fromField, @"<([^>]+)>");
        if (match.Success) return match.Groups[1].Value.Trim();
        return fromField.Trim();
    }
}
