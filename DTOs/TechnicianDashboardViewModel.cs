using ServiceHub_IT.Models;

namespace ServiceHub_IT.DTOs;

public class TechnicianDashboardViewModel
{
    public IReadOnlyList<Ticket> Tickets { get; set; } = Array.Empty<Ticket>();
    public IReadOnlyList<Asset> Assets { get; set; } = Array.Empty<Asset>();
    public int DueSoonCount { get; set; }
    public int CompletedCount { get; set; }
}
