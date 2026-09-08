using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Infrastructure.Services;

public class ResendEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(HttpClient httpClient, IConfiguration configuration, ILogger<ResendEmailService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendTicketCreatedAsync(int ticketId, string studentEmail, string studentName, string subject)
    {
        var htmlContent = $@"
        <div style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e5e7eb; border-radius: 8px; overflow: hidden;"">
            <div style=""background-color: #0f172a; padding: 24px; text-align: center;"">
                <h1 style=""color: #ffffff; margin: 0; font-size: 24px;"">Nexas Sistemas</h1>
            </div>
            <div style=""padding: 32px 24px; background-color: #ffffff;"">
                <h2 style=""color: #1e293b; font-size: 20px; margin-top: 0;"">Olá, {studentName}!</h2>
                <p style=""color: #475569; font-size: 16px; line-height: 1.5;"">
                    Seu chamado <strong>#{ticketId}</strong> (""{subject}"") foi aberto com sucesso em nossa plataforma.
                </p>
                <div style=""background-color: #f8fafc; border-left: 4px solid #f16363ff; padding: 16px; margin: 24px 0; border-radius: 4px;"">
                    <p style=""color: #334155; margin: 0; font-size: 15px; font-weight: 500;"">
                        Em até 24h úteis nossa equipe especializada entrará em contato com você.
                    </p>
                </div>
                <p style=""color: #475569; font-size: 15px; line-height: 1.5;"">
                    Você pode acompanhar o andamento ou enviar anexos adicionais diretamente pelo Portal de Suporte.
                </p>
                <div style=""margin-top: 32px;"">
                    <a href=""https://portalnexas.com.br/portal-pat/support/tickets"" style=""background-color: #f16363ff; color: #ffffff; text-decoration: none; padding: 12px 24px; border-radius: 6px; font-weight: 600; display: inline-block;"">
                        Acompanhar Meu Chamado
                    </a>
                </div>
            </div>
            <div style=""background-color: #f1f5f9; padding: 16px 24px; text-align: center; border-top: 1px solid #e5e7eb;"">
                <p style=""color: #94a3b8; font-size: 13px; margin: 0;"">
                    © {DateTime.Now.Year} Nexas Sistemas. Todos os direitos reservados.<br>
                    Este é um e-mail automático, por favor não responda.
                </p>
            </div>
        </div>";

        await SendEmailAsync(studentEmail, $"Chamado #{ticketId} Aberto - {subject}", htmlContent);
    }

    public async Task SendTicketReplyAsync(int ticketId, string studentEmail, string studentName, string replyContent)
    {
        // Placeholder for future implementation
        await Task.CompletedTask;
    }

    public async Task SendTicketClosedAsync(int ticketId, string studentEmail, string studentName)
    {
        // Placeholder for future implementation
        await Task.CompletedTask;
    }

    public async Task SendTicketStatusChangedAsync(int ticketId, string studentEmail, string studentName, string newStatus)
    {
        // Placeholder for future implementation
        await Task.CompletedTask;
    }

    public async Task SendTicketUnregisteredUserAsync(string toEmail)
    {
        var htmlContent = $@"
        <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee;"">
            <h2 style=""color: #333;"">Nexas Sistemas - Aviso</h2>
            <p>Olá,</p>
            <p>Recebemos uma mensagem sua em nosso canal de suporte, porém não encontramos nenhuma conta Nexas associada a este endereço de e-mail ({toEmail}).</p>
            <p>Por motivos de segurança e organização, só abrimos chamados para e-mails cadastrados. Por favor, envie sua dúvida a partir do e-mail cadastrado ou faça login no portal e crie um chamado por lá.</p>
        </div>";
        await SendEmailAsync(toEmail, "Nexas - E-mail não cadastrado", htmlContent);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
    {
        var apiKey = _configuration["EmailSettings:apikey"] ?? _configuration["EmailSettings:ApiKey"];
        var senderEmail = _configuration["EmailSettings:senderemail"] ?? _configuration["EmailSettings:SenderEmail"] ?? "suporte@portalnexas.com.br";
        var senderName = _configuration["EmailSettings:sendername"] ?? _configuration["EmailSettings:SenderName"] ?? "Nexas Suporte";

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Resend API Key is missing. Email will not be sent.");
            return;
        }

        var payload = new
        {
            from = $"{senderName} <{senderEmail}>",
            to = new[] { toEmail },
            subject = subject,
            html = htmlContent
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        try
        {
            var response = await _httpClient.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Failed to send email via Resend: {error}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while sending email via Resend");
        }
    }
}
