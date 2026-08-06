using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub_IT.DTOs;
using ServiceHub_IT.Models;
using ServiceHub_IT.Services;

namespace ServiceHub_IT.Controllers;

[Authorize(Roles = "Employee")]
public class EmployeeController : Controller
{
    private readonly TicketService _ticketService;
    private readonly AssetService _assetService;

    public EmployeeController(TicketService ticketService, AssetService assetService)
    {
        _ticketService = ticketService;
        _assetService = assetService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var email = User.Identity?.Name ?? User.FindFirst("email")?.Value ?? string.Empty;
        var tickets = (await _ticketService.GetTicketsAsync(requester: email, cancellationToken: cancellationToken)).ToList();
        var assets = (await _assetService.GetAssetsAsync(new AssetSearchViewModel(), cancellationToken)).ToList();
        var assignedAssets = assets.Where(a => string.Equals(a.assigned_employee, email, StringComparison.OrdinalIgnoreCase)).ToList();

        var model = new EmployeeDashboardViewModel
        {
            Tickets = tickets,
            Assets = assignedAssets,
            ResolvedCount = tickets.Count(t => string.Equals(t.status, "Resolved", StringComparison.OrdinalIgnoreCase) || string.Equals(t.status, "Closed", StringComparison.OrdinalIgnoreCase)),
            PendingCount = tickets.Count(t => !string.Equals(t.status, "Resolved", StringComparison.OrdinalIgnoreCase) && !string.Equals(t.status, "Closed", StringComparison.OrdinalIgnoreCase)),
            AssignedTechnician = tickets.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.assigned_technician))?.assigned_technician
        };

        return View(model);
    }
}
