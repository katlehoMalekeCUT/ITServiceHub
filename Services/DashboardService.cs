using System.Text.Json;
using Microsoft.Extensions.Options;
using ServiceHub_IT.DTOs;
using ServiceHub_IT.Models;

namespace ServiceHub_IT.Services;

public class DashboardService
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseSettings _settings;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(HttpClient httpClient, IOptions<SupabaseSettings> options, ILogger<DashboardService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<DashboardMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var metrics = new DashboardMetricsDto();

        if (string.IsNullOrWhiteSpace(_settings.Url) || string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            return metrics;
        }

        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");

            using var assetsResponse = await _httpClient.GetAsync($"{_settings.Url.TrimEnd('/')}/rest/v1/assets?select=*", cancellationToken);
            if (assetsResponse.IsSuccessStatusCode)
            {
                var assets = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(await assetsResponse.Content.ReadAsStringAsync(cancellationToken));
                if (assets is not null)
                {
                    metrics.TotalAssets = assets.Count;
                    metrics.AssignedAssets = assets.Count(x => x.TryGetValue("assigned_employee", out var assignedEmployee) && !string.IsNullOrWhiteSpace(assignedEmployee.GetString()));
                    metrics.AvailableAssets = assets.Count - metrics.AssignedAssets;
                }
            }

            using var profilesResponse = await _httpClient.GetAsync($"{_settings.Url.TrimEnd('/')}/rest/v1/profiles?select=*", cancellationToken);
            if (profilesResponse.IsSuccessStatusCode)
            {
                var profiles = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(await profilesResponse.Content.ReadAsStringAsync(cancellationToken));
                if (profiles is not null)
                {
                    metrics.Employees = profiles.Count(x => x.TryGetValue("role", out var role) && role.GetString()?.Equals("Employee", StringComparison.OrdinalIgnoreCase) == true);
                    metrics.Technicians = profiles.Count(x => x.TryGetValue("role", out var role) && role.GetString()?.Equals("Technician", StringComparison.OrdinalIgnoreCase) == true);
                }
            }

            using var ticketsResponse = await _httpClient.GetAsync($"{_settings.Url.TrimEnd('/')}/rest/v1/tickets?select=*", cancellationToken);
            if (ticketsResponse.IsSuccessStatusCode)
            {
                var tickets = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(await ticketsResponse.Content.ReadAsStringAsync(cancellationToken));
                if (tickets is not null)
                {
                    var parsedTickets = tickets.Select(TicketFromPayload).ToList();
                    metrics.TotalTickets = parsedTickets.Count;
                    metrics.OpenTickets = parsedTickets.Count(x => x.Status.Equals("Open", StringComparison.OrdinalIgnoreCase));
                    metrics.InProgressTickets = parsedTickets.Count(x => x.Status.Equals("In Progress", StringComparison.OrdinalIgnoreCase));
                    metrics.ResolvedTickets = parsedTickets.Count(x => x.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase));
                    metrics.ClosedTickets = parsedTickets.Count(x => x.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase));
                    metrics.HighPriorityTickets = parsedTickets.Count(x => x.Priority.Equals("High", StringComparison.OrdinalIgnoreCase));
                    var now = DateTime.UtcNow;
                    metrics.TicketsCreatedToday = parsedTickets.Count(x => x.CreatedAt is not null && x.CreatedAt.Value.Date == now.Date);
                    metrics.TicketsCreatedThisWeek = parsedTickets.Count(x => x.CreatedAt is not null && x.CreatedAt.Value >= now.AddDays(-7));
                    var resolvedTickets = parsedTickets.Where(x => x.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase) || x.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase)).ToList();
                    metrics.ResolutionRate = parsedTickets.Count == 0 ? 0 : Math.Round(resolvedTickets.Count * 100.0 / parsedTickets.Count, 1);
                    metrics.AverageResolutionHours = parsedTickets.Count == 0 ? 0 : Math.Round(parsedTickets.Average(x => x.ResolutionHours), 1);
                    metrics.StatusBreakdown = parsedTickets.GroupBy(x => x.Status).Select(x => new TicketStatusChartItem { Label = string.IsNullOrWhiteSpace(x.Key) ? "Unknown" : x.Key, Count = x.Count() }).OrderByDescending(x => x.Count).ToList();
                    metrics.PriorityBreakdown = parsedTickets.GroupBy(x => x.Priority).Select(x => new TicketPriorityChartItem { Label = string.IsNullOrWhiteSpace(x.Key) ? "Unknown" : x.Key, Count = x.Count() }).OrderByDescending(x => x.Count).ToList();
                    metrics.MonthlyTrend = parsedTickets.Where(x => x.CreatedAt is not null).GroupBy(x => x.CreatedAt!.Value.ToString("MMM yyyy")).Select(x => new TicketTrendItem { Label = x.Key, Count = x.Count() }).OrderBy(x => x.Label).ToList();
                }
            }

            using var departmentsResponse = await _httpClient.GetAsync($"{_settings.Url.TrimEnd('/')}/rest/v1/departments?select=*", cancellationToken);
            if (departmentsResponse.IsSuccessStatusCode)
            {
                var departments = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(await departmentsResponse.Content.ReadAsStringAsync(cancellationToken));
                if (departments is not null)
                {
                    metrics.TotalDepartments = departments.Count;
                }
            }

            using var maintenanceResponse = await _httpClient.GetAsync($"{_settings.Url.TrimEnd('/')}/rest/v1/maintenance?select=*", cancellationToken);
            if (maintenanceResponse.IsSuccessStatusCode)
            {
                var maintenance = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(await maintenanceResponse.Content.ReadAsStringAsync(cancellationToken));
                if (maintenance is not null)
                {
                    metrics.MaintenanceDue = maintenance.Count(x => x.TryGetValue("status", out var status) && status.GetString()?.Equals("Due", StringComparison.OrdinalIgnoreCase) == true);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to load dashboard metrics from Supabase.");
        }

        return metrics;
    }

    private static TicketSummary TicketFromPayload(Dictionary<string, JsonElement> payload)
    {
        var status = payload.TryGetValue("status", out var statusValue) ? statusValue.GetString() : "Open";
        var priority = payload.TryGetValue("priority", out var priorityValue) ? priorityValue.GetString() : "Medium";
        var createdAt = payload.TryGetValue("created_at", out var createdAtValue) && createdAtValue.ValueKind != JsonValueKind.Null ? createdAtValue.GetDateTime() : (DateTime?)null;
        var resolvedAt = payload.TryGetValue("updated_at", out var updatedAtValue) && updatedAtValue.ValueKind != JsonValueKind.Null ? updatedAtValue.GetDateTime() : createdAt;
        var resolutionHours = createdAt is null || resolvedAt is null ? 0 : Math.Round((resolvedAt.Value - createdAt.Value).TotalHours, 1);

        return new TicketSummary(status ?? "Open", priority ?? "Medium", createdAt, resolutionHours);
    }

    private sealed record TicketSummary(string Status, string Priority, DateTime? CreatedAt, double ResolutionHours);
}
