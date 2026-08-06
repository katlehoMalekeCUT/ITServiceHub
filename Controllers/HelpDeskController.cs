using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub_IT.Interfaces;
using ServiceHub_IT.Models;
using ServiceHub_IT.Services;
using ServiceHub_IT.Services.Ai;

namespace ServiceHub_IT.Controllers;

[Authorize]
public class HelpDeskController : Controller
{
    private readonly TicketService _ticketService;
    private readonly TicketPredictionModel _ticketPredictionModel;
    private readonly ISupabaseService _supabaseService;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<HelpDeskController> _logger;

    public HelpDeskController(TicketService ticketService, TicketPredictionModel ticketPredictionModel, ISupabaseService supabaseService, IEmailService emailService, INotificationService notificationService, ILogger<HelpDeskController> logger)
    {
        _ticketService = ticketService;
        _ticketPredictionModel = ticketPredictionModel;
        _supabaseService = supabaseService;
        _emailService = emailService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var user = User.Identity?.Name ?? User.FindFirst("email")?.Value ?? string.Empty;
        var isAdmin = User.IsInRole("Admin");
        var isTechnician = User.IsInRole("Technician") || User.IsInRole("Admin");

        IReadOnlyList<Ticket> tickets;
        if (isAdmin)
        {
            tickets = await _ticketService.GetTicketsAsync(cancellationToken: cancellationToken);
        }
        else if (isTechnician)
        {
            tickets = await _ticketService.GetTicketsAsync(assignedTechnician: user, cancellationToken: cancellationToken);
        }
        else
        {
            tickets = await _ticketService.GetTicketsAsync(requester: user, cancellationToken: cancellationToken);
        }

        return View(tickets);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateTicketViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTicketViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = User.Identity?.Name ?? User.FindFirst("email")?.Value ?? string.Empty;
        var screenshotUrl = await UploadScreenshotAsync(model.Screenshot, cancellationToken);

        var ticket = new Ticket
        {
            title = model.Title,
            description = model.Description,
            requester = user,
            priority = model.Priority,
            category = model.Category,
            screenshot_url = screenshotUrl,
            comments = string.Empty,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        var created = await _ticketService.CreateTicketAsync(ticket, cancellationToken);
        if (!created)
        {
            TempData["Error"] = "Unable to create the ticket right now.";
            return View(model);
        }

        await _emailService.SendTicketCreatedEmailAsync(user, ticket.title, ticket.id ?? "new-ticket", cancellationToken);
        await _notificationService.CreateNotificationAsync("Admin", "New ticket created", $"{user} created a new ticket: {ticket.title}", cancellationToken);
        TempData["Success"] = "Ticket created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.GetTicketAsync(id, cancellationToken);
        if (ticket is null)
        {
            return NotFound();
        }

        var prediction = _ticketPredictionModel.Predict(ticket.title, ticket.description, ticket.category, ticket.priority);
        ViewBag.AiPrediction = prediction;

        var technicians = (await _supabaseService.GetAllProfilesAsync(cancellationToken))
            .Where(p => string.Equals(p.Role, "Technician", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.FullName)
            .ThenBy(p => p.Email)
            .ToList();
        ViewBag.Technicians = technicians;

        return View(ticket);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignTicket(string id, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.GetTicketAsync(id, cancellationToken);
        if (ticket is null)
        {
            return NotFound();
        }

        var user = User.Identity?.Name ?? User.FindFirst("email")?.Value ?? string.Empty;
        ticket.assigned_technician = user;
        ticket.assigned_technician_id = user;
        ticket.assigned_date = DateTime.UtcNow;
        ticket.status = "Assigned";
        ticket.updated_at = DateTime.UtcNow;
        ticket.comments = string.IsNullOrWhiteSpace(ticket.comments)
            ? $"{user}: assigned ticket"
            : $"{ticket.comments}\n{user}: assigned ticket";

        var updated = await _ticketService.UpdateTicketAsync(ticket, cancellationToken);
        if (!updated)
        {
            TempData["Error"] = "Unable to assign the ticket.";
            return RedirectToAction(nameof(Details), new { id });
        }

        await _emailService.SendTicketAssignedEmailAsync(user, ticket.title, ticket.id ?? id, cancellationToken);
        await _notificationService.CreateNotificationAsync(user, "Ticket assigned", $"You were assigned to ticket: {ticket.title}", cancellationToken);
        TempData["Success"] = "Ticket assigned.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptTicket(string id, CancellationToken cancellationToken)
    {
        return await AssignTicket(id, cancellationToken);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(string id, UpdateTicketStatusViewModel model, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.GetTicketAsync(id, cancellationToken);
        if (ticket is null)
        {
            return NotFound();
        }

        var user = User.Identity?.Name ?? User.FindFirst("email")?.Value ?? string.Empty;
        ticket.status = model.Status;
        ticket.updated_at = DateTime.UtcNow;
        ticket.due_date = model.DueDate;
        if (!string.IsNullOrWhiteSpace(model.AssignedTechnician))
        {
            ticket.assigned_technician = model.AssignedTechnician;
            ticket.assigned_technician_id = model.AssignedTechnician;
            ticket.assigned_date = DateTime.UtcNow;
            if (string.Equals(model.Status, "Open", StringComparison.OrdinalIgnoreCase))
            {
                ticket.status = "Assigned";
            }
        }

        if (!string.IsNullOrWhiteSpace(model.Comment))
        {
            ticket.comments = string.IsNullOrWhiteSpace(ticket.comments)
                ? $"{user}: {model.Comment}"
                : $"{ticket.comments}\n{user}: {model.Comment}";
        }

        var updated = await _ticketService.UpdateTicketAsync(ticket, cancellationToken);
        if (!updated)
        {
            TempData["Error"] = "Unable to update the ticket.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (string.Equals(ticket.status, "Resolved", StringComparison.OrdinalIgnoreCase) || string.Equals(ticket.status, "Closed", StringComparison.OrdinalIgnoreCase))
        {
            await _emailService.SendTicketResolvedEmailAsync(ticket.requester, ticket.title, ticket.id ?? id, cancellationToken);
        }
        else
        {
            await _emailService.SendTicketStatusChangedEmailAsync(ticket.requester, ticket.title, ticket.id ?? id, ticket.status, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(ticket.assigned_technician))
        {
            await _notificationService.CreateNotificationAsync(ticket.assigned_technician, "Ticket updated", $"The ticket {ticket.title} has a new status: {ticket.status}", cancellationToken);
        }

        TempData["Success"] = "Ticket updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<string?> UploadScreenshotAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        var fileName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
        return await _ticketService.UploadScreenshotAsync(file, fileName, cancellationToken);
    }
}
