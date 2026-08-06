using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub_IT.Interfaces;
using ServiceHub_IT.Models;

namespace ServiceHub_IT.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var notifications = await _notificationService.GetNotificationsAsync(userId, cancellationToken);
        return View(notifications);
    }

    [HttpGet]
    public async Task<IActionResult> UnreadCount(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var unreadCount = await _notificationService.GetUnreadCountAsync(userId, cancellationToken);
        return Json(new { count = unreadCount });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return RedirectToAction(nameof(Index));
        }

        await _notificationService.MarkAsReadAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private string GetCurrentUserId()
    {
        return User.Identity?.Name ?? User.FindFirst("email")?.Value ?? string.Empty;
    }
}
