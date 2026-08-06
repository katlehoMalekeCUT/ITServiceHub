using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub_IT.DTOs;
using ServiceHub_IT.Models;
using ServiceHub_IT.Services;

namespace ServiceHub_IT.Controllers;

[Authorize(Roles = "Technician")]
public class TechnicianController : Controller
{
    private readonly TicketService _ticketService;
    private readonly AssetService _assetService;

    public TechnicianController(TicketService ticketService, AssetService assetService)
    {
        _ticketService = ticketService;
        _assetService = assetService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var email = User.Identity?.Name ?? User.FindFirst("email")?.Value ?? string.Empty;
        var tickets = (await _ticketService.GetTicketsAsync(assignedTechnician: email, cancellationToken: cancellationToken)).ToList();
        var assets = (await _assetService.GetAssetsAsync(new AssetSearchViewModel(), cancellationToken)).ToList();
        var assignedAssets = assets.Where(a => string.Equals(a.assigned_employee, email, StringComparison.OrdinalIgnoreCase)).ToList();

        var model = new TechnicianDashboardViewModel
        {
            Tickets = tickets,
            Assets = assignedAssets,
            CompletedCount = tickets.Count(t => string.Equals(t.status, "Resolved", StringComparison.OrdinalIgnoreCase) || string.Equals(t.status, "Closed", StringComparison.OrdinalIgnoreCase)),
            DueSoonCount = tickets.Count(t => !string.Equals(t.status, "Resolved", StringComparison.OrdinalIgnoreCase) && !string.Equals(t.status, "Closed", StringComparison.OrdinalIgnoreCase))
        };

        return View(model);
    }
}
