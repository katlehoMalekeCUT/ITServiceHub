using Microsoft.EntityFrameworkCore;
using ServiceHub_IT.Data;
using ServiceHub_IT.Interfaces;
using ServiceHub_IT.Models;

namespace ServiceHub_IT.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<NotificationItem>> GetNotificationsAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Array.Empty<NotificationItem>();
        }

        return await _context.Notifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return 0;
        }

        return await _context.Notifications.CountAsync(x => x.UserId == userId && !x.IsRead, cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications.FindAsync(new object?[] { notificationId }, cancellationToken);
        if (notification is null)
        {
            return;
        }

        notification.IsRead = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateNotificationAsync(string userId, string title, string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        _context.Notifications.Add(new NotificationItem
        {
            UserId = userId,
            Title = title,
            Message = message,
            IsRead = false
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
