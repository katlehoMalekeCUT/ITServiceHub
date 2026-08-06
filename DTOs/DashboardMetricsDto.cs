namespace ServiceHub_IT.DTOs;

public class DashboardMetricsDto
{
    public int TotalAssets { get; set; }
    public int AssignedAssets { get; set; }
    public int AvailableAssets { get; set; }
    public int Employees { get; set; }
    public int Technicians { get; set; }
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int ClosedTickets { get; set; }
    public int HighPriorityTickets { get; set; }
    public int TicketsCreatedToday { get; set; }
    public int TicketsCreatedThisWeek { get; set; }
    public double ResolutionRate { get; set; }
    public double AverageResolutionHours { get; set; }
    public int TotalDepartments { get; set; }
    public int MaintenanceDue { get; set; }
    public IReadOnlyList<TicketStatusChartItem> StatusBreakdown { get; set; } = Array.Empty<TicketStatusChartItem>();
    public IReadOnlyList<TicketPriorityChartItem> PriorityBreakdown { get; set; } = Array.Empty<TicketPriorityChartItem>();
    public IReadOnlyList<TicketTrendItem> MonthlyTrend { get; set; } = Array.Empty<TicketTrendItem>();
}
