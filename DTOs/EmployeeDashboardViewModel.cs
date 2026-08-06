using ServiceHub_IT.Models;

namespace ServiceHub_IT.DTOs;

public class EmployeeDashboardViewModel
{
    public IReadOnlyList<Ticket> Tickets { get; set; } = Array.Empty<Ticket>();
    public IReadOnlyList<Asset> Assets { get; set; } = Array.Empty<Asset>();
    public int ResolvedCount { get; set; }
    public int PendingCount { get; set; }
    public string? AssignedTechnician { get; set; }
}
