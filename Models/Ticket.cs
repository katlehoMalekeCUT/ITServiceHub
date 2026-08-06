namespace ServiceHub_IT.Models;

public class Ticket
{
    public string? id { get; set; }
    public string title { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public string requester { get; set; } = string.Empty;
    public string priority { get; set; } = "Medium";
    public string status { get; set; } = "Open";
    public string? assigned_technician { get; set; }
    public string? assigned_technician_id { get; set; }
    public DateTime? assigned_date { get; set; }
    public string category { get; set; } = string.Empty;
    public string? screenshot_url { get; set; }
    public string? comments { get; set; }
    public DateTime? due_date { get; set; }
    public DateTime? created_at { get; set; } = DateTime.UtcNow;
    public DateTime? updated_at { get; set; } = DateTime.UtcNow;
}
