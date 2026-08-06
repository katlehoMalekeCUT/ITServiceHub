using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub_IT.DTOs;
using ServiceHub_IT.Models;
using ServiceHub_IT.Services;
using ServiceHub_IT.Interfaces;

namespace ServiceHub_IT.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly TicketService _ticketService;
    private readonly AssetService _assetService;
    private readonly ISupabaseService _supabaseService;

    public AdminController(TicketService ticketService, AssetService assetService, ISupabaseService supabaseService)
    {
        _ticketService = ticketService;
        _assetService = assetService;
        _supabaseService = supabaseService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var tickets = (await _ticketService.GetTicketsAsync(cancellationToken: cancellationToken)).ToList();
        var assets = (await _assetService.GetAssetsAsync(new AssetSearchViewModel(), cancellationToken)).ToList();
        var profiles = (await _supabaseService.GetAllProfilesAsync(cancellationToken)).ToList();

        var employeeProfiles = profiles.Where(p => string.Equals(p.Role, "Employee", StringComparison.OrdinalIgnoreCase)).ToList();
        var technicianProfiles = profiles.Where(p => string.Equals(p.Role, "Technician", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Role, "Admin", StringComparison.OrdinalIgnoreCase)).ToList();

        var technicianTicketCounts = tickets
            .Where(t => !string.IsNullOrWhiteSpace(t.assigned_technician))
            .GroupBy(t => t.assigned_technician!, StringComparer.OrdinalIgnoreCase)
            .Select(g => new { Technician = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        var topTechnician = technicianTicketCounts.FirstOrDefault();
        var lowestTechnician = technicianTicketCounts.LastOrDefault();

        var model = new AdminDashboardViewModel
        {
            Employees = employeeProfiles.Count,
            Technicians = technicianProfiles.Count,
            ResolvedTickets = tickets.Count(t => string.Equals(t.status, "Resolved", StringComparison.OrdinalIgnoreCase) || string.Equals(t.status, "Closed", StringComparison.OrdinalIgnoreCase)),
            UnresolvedTickets = tickets.Count(t => !string.Equals(t.status, "Resolved", StringComparison.OrdinalIgnoreCase) && !string.Equals(t.status, "Closed", StringComparison.OrdinalIgnoreCase)),
            TopTechnician = topTechnician?.Technician,
            TopTechnicianCount = topTechnician?.Count ?? 0,
            LowestTechnician = lowestTechnician?.Technician,
            LowestTechnicianCount = lowestTechnician?.Count ?? 0,
            Tickets = tickets,
            Profiles = profiles,
            Assets = assets
        };

        return View(model);
    }
}
