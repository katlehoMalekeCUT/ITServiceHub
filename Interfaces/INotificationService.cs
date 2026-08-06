namespace ServiceHub_IT.Interfaces;

public interface INotificationService
{
    Task<IReadOnlyList<ServiceHub_IT.Models.NotificationItem>> GetNotificationsAsync(string userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task CreateNotificationAsync(string userId, string title, string message, CancellationToken cancellationToken = default);
}
