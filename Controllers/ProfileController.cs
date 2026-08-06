using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub_IT.Interfaces;
using ServiceHub_IT.Models;

namespace ServiceHub_IT.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ISupabaseService _supabaseService;

    public ProfileController(ISupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var email = User.Identity?.Name ?? User.FindFirst("email")?.Value ?? string.Empty;
        var profile = await _supabaseService.GetProfileByEmailAsync(email, cancellationToken);

        if (profile is null)
        {
            profile = new Profile
            {
                Email = email,
                FullName = email,
                Role = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Technician") ? "Technician" : "Employee"
            };
        }

        return View(profile);
    }
}
