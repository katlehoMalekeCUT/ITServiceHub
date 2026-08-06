namespace ServiceHub_IT.Models;

public class CreateTicketViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string Category { get; set; } = string.Empty;
    public IFormFile? Screenshot { get; set; }
}

public class UpdateTicketStatusViewModel
{
    public string Status { get; set; } = "Open";
    public string? AssignedTechnician { get; set; }
    public string? Comment { get; set; }
    public DateTime? DueDate { get; set; }
}
