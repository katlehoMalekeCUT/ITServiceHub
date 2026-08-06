namespace ServiceHub_IT.Interfaces;

public interface IEmailService
{
    Task SendTicketCreatedEmailAsync(string recipientEmail, string ticketTitle, string ticketId, CancellationToken cancellationToken = default);
    Task SendTicketAssignedEmailAsync(string recipientEmail, string ticketTitle, string ticketId, CancellationToken cancellationToken = default);
    Task SendTicketStatusChangedEmailAsync(string recipientEmail, string ticketTitle, string ticketId, string status, CancellationToken cancellationToken = default);
    Task SendTicketResolvedEmailAsync(string recipientEmail, string ticketTitle, string ticketId, CancellationToken cancellationToken = default);
}
