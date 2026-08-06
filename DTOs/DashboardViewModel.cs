using ServiceHub_IT.Models;

namespace ServiceHub_IT.DTOs;

public class DashboardViewModel
{
    public DashboardMetricsDto Metrics { get; set; } = new DashboardMetricsDto();
    public IReadOnlyList<Notification> LatestNotifications { get; set; } = Array.Empty<Notification>();
    public int UnreadNotificationsCount { get; set; }
}
