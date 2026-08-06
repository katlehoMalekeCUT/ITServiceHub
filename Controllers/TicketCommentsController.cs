using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub_IT.Data;
using ServiceHub_IT.Models;

namespace ServiceHub_IT.Controllers;

[Authorize]
public class TicketCommentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public TicketCommentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string ticketId, string message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ticketId) || string.IsNullOrWhiteSpace(message))
        {
            return RedirectToAction("Details", "HelpDesk", new { id = ticketId });
        }

        var comment = new TicketComment
        {
            TicketId = ticketId,
            UserName = User.Identity?.Name ?? User.FindFirst("email")?.Value ?? "System",
            UserRole = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Technician") ? "Technician" : "Employee",
            Message = message,
            CreatedAt = DateTime.UtcNow
        };

        _context.TicketComments.Add(comment);
        await _context.SaveChangesAsync(cancellationToken);
        return RedirectToAction("Details", "HelpDesk", new { id = ticketId });
    }
}
