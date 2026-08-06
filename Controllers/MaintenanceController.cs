using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub_IT.Interfaces;
using ServiceHub_IT.Models;
using ServiceHub_IT.Services;

namespace ServiceHub_IT.Controllers;

[Authorize(Roles = "Admin,Technician")]
public class MaintenanceController : Controller
{
    private readonly ISupabaseService _supabaseService;
    private readonly AssetService _assetService;
    private readonly ILogger<MaintenanceController> _logger;

    public MaintenanceController(ISupabaseService supabaseService, AssetService assetService, ILogger<MaintenanceController> logger)
    {
        _supabaseService = supabaseService;
        _assetService = assetService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var profileId = await GetCurrentProfileIdAsync(cancellationToken);
        var maintenance = (await _supabaseService.GetAllMaintenanceAsync(cancellationToken)).ToList();

        if (User.IsInRole("Technician") && !User.IsInRole("Admin"))
        {
            maintenance = maintenance.Where(x => x.technician_id == profileId).ToList();
        }

        var assets = (await _assetService.GetAssetsAsync(new AssetSearchViewModel(), cancellationToken))
            .Where(x => Guid.TryParse(x.id, out _))
            .ToDictionary(x => Guid.Parse(x.id!), x => x.asset_name);

        var profiles = (await _supabaseService.GetAllProfilesAsync(cancellationToken))
            .ToDictionary(x => x.Id, x => string.IsNullOrWhiteSpace(x.FullName) ? x.Email : x.FullName);

        var model = maintenance.Select(record => new MaintenanceRecordViewModel
        {
            Id = record.id,
            AssetName = assets.GetValueOrDefault(record.asset_id, record.asset_id.ToString()),
            TechnicianName = profiles.GetValueOrDefault(record.technician_id, record.technician_id.ToString()),
            Problem = record.problem,
            Solution = record.solution,
            MaintenanceDate = record.maintenance_date,
            NextServiceDate = record.next_service_date,
            MaintenanceCost = record.maintenance_cost,
            Status = record.status,
            CreatedAt = record.created_at
        }).OrderByDescending(x => x.MaintenanceDate).ToList();

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var maintenance = await _supabaseService.GetMaintenanceByIdAsync(id, cancellationToken);
        if (maintenance is null)
        {
            return NotFound();
        }

        if (User.IsInRole("Technician") && !User.IsInRole("Admin") && maintenance.technician_id != await GetCurrentProfileIdAsync(cancellationToken))
        {
            return Forbid();
        }

        var asset = await _assetService.GetAssetAsync(maintenance.asset_id.ToString(), cancellationToken);
        var technician = await _supabaseService.GetProfileByIdAsync(maintenance.technician_id, cancellationToken);

        ViewBag.AssetName = asset?.asset_name ?? maintenance.asset_id.ToString();
        ViewBag.TechnicianName = technician is not null ? (string.IsNullOrWhiteSpace(technician.FullName) ? technician.Email : technician.FullName) : maintenance.technician_id.ToString();

        return View(maintenance);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var assets = await _assetService.GetAssetsAsync(new AssetSearchViewModel(), cancellationToken);
        var technicians = await _supabaseService.GetAllProfilesAsync(cancellationToken);

        var model = new MaintenanceFormViewModel
        {
            Assets = assets.Where(x => !string.IsNullOrWhiteSpace(x.id) && Guid.TryParse(x.id, out _)),
            Technicians = technicians.Where(x => x.Role.Equals("Technician", StringComparison.OrdinalIgnoreCase) || x.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MaintenanceFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Assets = await _assetService.GetAssetsAsync(new AssetSearchViewModel(), cancellationToken);
            model.Technicians = (await _supabaseService.GetAllProfilesAsync(cancellationToken))
                .Where(x => x.Role.Equals("Technician", StringComparison.OrdinalIgnoreCase) || x.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase));
            return View(model);
        }

        var maintenance = new Maintenance
        {
            id = Guid.NewGuid(),
            asset_id = model.AssetId,
            technician_id = model.TechnicianId,
            problem = model.Problem,
            solution = model.Solution,
            maintenance_date = model.MaintenanceDate,
            next_service_date = model.NextServiceDate,
            maintenance_cost = model.MaintenanceCost,
            status = model.Status,
            created_at = DateTime.UtcNow
        };

        var success = await _supabaseService.CreateMaintenanceAsync(maintenance, cancellationToken);
        if (!success)
        {
            TempData["ErrorMessage"] = "Unable to create the maintenance record right now.";
            model.Assets = await _assetService.GetAssetsAsync(new AssetSearchViewModel(), cancellationToken);
            model.Technicians = (await _supabaseService.GetAllProfilesAsync(cancellationToken))
                .Where(x => x.Role.Equals("Technician", StringComparison.OrdinalIgnoreCase) || x.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase));
            return View(model);
        }

        TempData["SuccessMessage"] = "Maintenance record created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var maintenance = await _supabaseService.GetMaintenanceByIdAsync(id, cancellationToken);
        if (maintenance is null)
        {
            return NotFound();
        }

        if (User.IsInRole("Technician") && !User.IsInRole("Admin") && maintenance.technician_id != await GetCurrentProfileIdAsync(cancellationToken))
        {
            return Forbid();
        }

        var assets = await _assetService.GetAssetsAsync(new AssetSearchViewModel(), cancellationToken);
        var technicians = await _supabaseService.GetAllProfilesAsync(cancellationToken);

        var model = new MaintenanceFormViewModel
        {
            Id = maintenance.id,
            AssetId = maintenance.asset_id,
            TechnicianId = maintenance.technician_id,
            Problem = maintenance.problem,
            Solution = maintenance.solution,
            MaintenanceDate = maintenance.maintenance_date,
            NextServiceDate = maintenance.next_service_date,
            MaintenanceCost = maintenance.maintenance_cost,
            Status = maintenance.status,
            Assets = assets.Where(x => !string.IsNullOrWhiteSpace(x.id) && Guid.TryParse(x.id, out _)),
            Technicians = technicians.Where(x => x.Role.Equals("Technician", StringComparison.OrdinalIgnoreCase) || x.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, MaintenanceFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Assets = await _assetService.GetAssetsAsync(new AssetSearchViewModel(), cancellationToken);
            model.Technicians = (await _supabaseService.GetAllProfilesAsync(cancellationToken))
                .Where(x => x.Role.Equals("Technician", StringComparison.OrdinalIgnoreCase) || x.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase));
            return View(model);
        }

        var maintenance = await _supabaseService.GetMaintenanceByIdAsync(id, cancellationToken);
        if (maintenance is null)
        {
            return NotFound();
        }

        if (User.IsInRole("Technician") && !User.IsInRole("Admin") && maintenance.technician_id != await GetCurrentProfileIdAsync(cancellationToken))
        {
            return Forbid();
        }

        maintenance.asset_id = model.AssetId;
        maintenance.technician_id = model.TechnicianId;
        maintenance.problem = model.Problem;
        maintenance.solution = model.Solution;
        maintenance.maintenance_date = model.MaintenanceDate;
        maintenance.next_service_date = model.NextServiceDate;
        maintenance.maintenance_cost = model.MaintenanceCost;
        maintenance.status = model.Status;

        var success = await _supabaseService.UpdateMaintenanceAsync(maintenance, cancellationToken);
        if (!success)
        {
            TempData["ErrorMessage"] = "Unable to update the maintenance record right now.";
            model.Assets = await _assetService.GetAssetsAsync(new AssetSearchViewModel(), cancellationToken);
            model.Technicians = (await _supabaseService.GetAllProfilesAsync(cancellationToken))
                .Where(x => x.Role.Equals("Technician", StringComparison.OrdinalIgnoreCase) || x.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase));
            return View(model);
        }

        TempData["SuccessMessage"] = "Maintenance record updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var maintenance = await _supabaseService.GetMaintenanceByIdAsync(id, cancellationToken);
        if (maintenance is null)
        {
            return NotFound();
        }

        if (User.IsInRole("Technician") && !User.IsInRole("Admin") && maintenance.technician_id != await GetCurrentProfileIdAsync(cancellationToken))
        {
            return Forbid();
        }

        return View(maintenance);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        var maintenance = await _supabaseService.GetMaintenanceByIdAsync(id, cancellationToken);
        if (maintenance is null)
        {
            return NotFound();
        }

        if (User.IsInRole("Technician") && !User.IsInRole("Admin") && maintenance.technician_id != await GetCurrentProfileIdAsync(cancellationToken))
        {
            return Forbid();
        }

        var success = await _supabaseService.DeleteMaintenanceAsync(id, cancellationToken);
        if (!success)
        {
            TempData["ErrorMessage"] = "Unable to delete the maintenance record right now.";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Maintenance record deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<Guid> GetCurrentProfileIdAsync(CancellationToken cancellationToken)
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(idClaim) && Guid.TryParse(idClaim, out var profileId))
        {
            return profileId;
        }

        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrWhiteSpace(email))
        {
            var profile = await _supabaseService.GetProfileByEmailAsync(email, cancellationToken);
            if (profile is not null)
            {
                return profile.Id;
            }
        }

        return Guid.Empty;
    }
}
