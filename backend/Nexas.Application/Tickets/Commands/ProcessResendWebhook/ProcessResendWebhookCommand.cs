using MediatR;
using System.Collections.Generic;

namespace Nexas.Application.Tickets.Commands.ProcessResendWebhook;

public class ProcessResendWebhookCommand : IRequest<bool>
{
    public string Type { get; set; } = string.Empty;
    public ResendEmailData Data { get; set; } = new();
}

public class ResendEmailData
{
    public string From { get; set; } = string.Empty;
    public List<string> To { get; set; } = new();
    public string Subject { get; set; } = string.Empty;
    public string Html { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
