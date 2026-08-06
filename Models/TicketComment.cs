namespace ServiceHub_IT.Models;

public class TicketComment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TicketId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
