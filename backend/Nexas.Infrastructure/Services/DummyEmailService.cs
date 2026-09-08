using System.Threading.Tasks;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Infrastructure.Services;

public class DummyEmailService : IEmailService
{
    public Task SendTicketCreatedAsync(int ticketId, string studentEmail, string studentName, string subject)
        => Task.CompletedTask;

    public Task SendTicketReplyAsync(int ticketId, string studentEmail, string studentName, string replyContent)
        => Task.CompletedTask;

    public Task SendTicketClosedAsync(int ticketId, string studentEmail, string studentName)
        => Task.CompletedTask;

    public Task SendTicketStatusChangedAsync(int ticketId, string studentEmail, string studentName, string newStatus)
        => Task.CompletedTask;

    public Task SendTicketUnregisteredUserAsync(string toEmail)
    {
        System.Console.WriteLine($"[EmailService] Avisando {toEmail} que não está cadastrado no Portal Nexas.");
        return Task.CompletedTask;
    }
}
