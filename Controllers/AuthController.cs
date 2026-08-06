using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub_IT.Interfaces;
using ServiceHub_IT.Models;
using ServiceHub_IT.Utilities;

namespace ServiceHub_IT.Controllers;

[AllowAnonymous]
public class AuthController : Controller
{
    private readonly ISupabaseService _supabaseService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ISupabaseService supabaseService, ILogger<AuthController> logger)
    {
        _supabaseService = supabaseService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Dashboard", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!_supabaseService.IsConfigured)
        {
            ModelState.AddModelError(string.Empty, "Authentication is not configured.");
            return View(model);
        }

        try
        {
            var signInResult = await _supabaseService.SignInAsync(model.Email, model.Password, HttpContext.RequestAborted);
            if (!signInResult.IsSuccess || string.IsNullOrWhiteSpace(signInResult.AccessToken))
            {
                ModelState.AddModelError(string.Empty, signInResult.ErrorMessage ?? "Invalid email or password.");
                return View(model);
            }

            var rawRole = await _supabaseService.GetRoleForEmailAsync(model.Email, signInResult.AccessToken, HttpContext.RequestAborted);
            var normalizedRole = rawRole?.Trim() ?? string.Empty;
            normalizedRole = normalizedRole.Equals("admin", StringComparison.OrdinalIgnoreCase)
                ? "Admin"
                : normalizedRole.Equals("technician", StringComparison.OrdinalIgnoreCase)
                    ? "Technician"
                    : "Employee";

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, model.Email),
                new(ClaimTypes.Email, model.Email),
                new(ClaimTypes.Role, normalizedRole)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddDays(7)
            });

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            var dashboardController = RoleDashboardRoute.GetControllerName(normalizedRole);
            return RedirectToAction("Index", dashboardController);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Login failed.");
            ModelState.AddModelError(string.Empty, "Unable to sign in right now. Please try again later.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult SignUp()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Dashboard", "Home");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignUp(SignUpViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!_supabaseService.IsConfigured)
        {
            ModelState.AddModelError(string.Empty, "Authentication is not configured.");
            return View(model);
        }

        try
        {
            var signUpResult = await _supabaseService.SignUpAsync(model.Email, model.Password, HttpContext.RequestAborted);
            if (signUpResult.IsSuccess)
            {
                TempData["SuccessMessage"] = "Account created successfully. Please sign in to continue.";
                return RedirectToAction(nameof(Login));
            }

            ModelState.AddModelError(string.Empty, signUpResult.ErrorMessage ?? "Unable to create your account. Please try again.");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Signup failed.");
            ModelState.AddModelError(string.Empty, "Unable to create your account right now. Please try again later.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult ResetPassword(string token) => View(new ResetPasswordViewModel { Token = token ?? string.Empty });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var success = await _supabaseService.ResetPasswordAsync(model.Token, model.Password, HttpContext.RequestAborted);
            if (success)
            {
                TempData["SuccessMessage"] = "Your password has been reset successfully.";
                return RedirectToAction(nameof(Login));
            }

            TempData["ErrorMessage"] = "The reset link is invalid or has expired.";
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Reset password failed.");
            TempData["ErrorMessage"] = "Unable to reset your password right now.";
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        Response.Cookies.Delete("sb-access-token");
        Response.Cookies.Delete("sb-refresh-token");
        return RedirectToAction(nameof(Login));
    }

}
