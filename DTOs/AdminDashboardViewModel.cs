using ServiceHub_IT.Models;

namespace ServiceHub_IT.DTOs;

public class AdminDashboardViewModel
{
    public int Employees { get; set; }
    public int Technicians { get; set; }
    public int ResolvedTickets { get; set; }
    public int UnresolvedTickets { get; set; }
    public string? TopTechnician { get; set; }
    public int TopTechnicianCount { get; set; }
    public string? LowestTechnician { get; set; }
    public int LowestTechnicianCount { get; set; }
    public IReadOnlyList<Ticket> Tickets { get; set; } = Array.Empty<Ticket>();
    public IReadOnlyList<Profile> Profiles { get; set; } = Array.Empty<Profile>();
    public IReadOnlyList<Asset> Assets { get; set; } = Array.Empty<Asset>();
}
