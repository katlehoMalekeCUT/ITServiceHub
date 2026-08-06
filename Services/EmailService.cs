using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using ServiceHub_IT.Interfaces;
using ServiceHub_IT.Models;

namespace ServiceHub_IT.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
    {
        _logger = logger;
        _settings = options.Value;
    }

    public async Task SendTicketCreatedEmailAsync(string recipientEmail, string ticketTitle, string ticketId, CancellationToken cancellationToken = default)
    {
        await SendAsync(recipientEmail, "ServiceHub IT ticket created", BuildTemplate("Ticket created", ticketTitle, ticketId, "A new support ticket has been created and is awaiting review."), cancellationToken);
    }

    public async Task SendTicketAssignedEmailAsync(string recipientEmail, string ticketTitle, string ticketId, CancellationToken cancellationToken = default)
    {
        await SendAsync(recipientEmail, "ServiceHub IT ticket assigned", BuildTemplate("Ticket assigned", ticketTitle, ticketId, "A technician has been assigned to your ticket."), cancellationToken);
    }

    public async Task SendTicketStatusChangedEmailAsync(string recipientEmail, string ticketTitle, string ticketId, string status, CancellationToken cancellationToken = default)
    {
        await SendAsync(recipientEmail, "ServiceHub IT ticket updated", BuildTemplate("Ticket updated", ticketTitle, ticketId, $"The ticket status has been updated to {status}."), cancellationToken);
    }

    public async Task SendTicketResolvedEmailAsync(string recipientEmail, string ticketTitle, string ticketId, CancellationToken cancellationToken = default)
    {
        await SendAsync(recipientEmail, "ServiceHub IT ticket resolved", BuildTemplate("Ticket resolved", ticketTitle, ticketId, "The ticket has been marked as resolved."), cancellationToken);
    }

    private async Task SendAsync(string recipientEmail, string subject, string body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.SmtpHost) || string.IsNullOrWhiteSpace(_settings.FromAddress))
        {
            _logger.LogInformation("Skipping email send because SMTP settings are not configured.");
            return;
        }

        try
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPassword)
            };

            using var message = new MailMessage(_settings.FromAddress, recipientEmail, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Email send failed for {Recipient}", recipientEmail);
        }
    }

    private static string BuildTemplate(string title, string ticketTitle, string ticketId, string summary)
    {
        return $@"
            <html>
            <body style='font-family:Segoe UI, sans-serif; color:#0f172a;'>
                <div style='max-width:640px; margin:0 auto; padding:24px; border:1px solid #e2e8f0; border-radius:16px;'>
                    <h2 style='margin-top:0;'>{title}</h2>
                    <p>{summary}</p>
                    <p><strong>Ticket:</strong> {ticketTitle}</p>
                    <p><strong>Reference:</strong> {ticketId}</p>
                    <p style='margin-top:16px;'>Thank you,<br/>ServiceHub IT</p>
                </div>
            </body>
            </html>";
    }
}

public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
}
