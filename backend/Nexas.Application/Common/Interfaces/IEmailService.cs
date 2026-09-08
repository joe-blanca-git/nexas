using System.Threading.Tasks;

namespace Nexas.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendTicketCreatedAsync(int ticketId, string userEmail, string userName, string subject);
    Task SendTicketReplyAsync(int ticketId, string userEmail, string userName, string replyContent);
    Task SendTicketClosedAsync(int ticketId, string userEmail, string userName);
    Task SendTicketStatusChangedAsync(int ticketId, string userEmail, string userName, string newStatus);
    Task SendTicketUnregisteredUserAsync(string toEmail);
}
