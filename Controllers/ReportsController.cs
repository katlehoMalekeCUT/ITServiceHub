using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub_IT.Models;
using System.Text;
using ServiceHub_IT.Services;

namespace ServiceHub_IT.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly ReportService _reportService;
    private readonly HttpClient _httpClient;
    private readonly SupabaseSettings _settings;

    public ReportsController(ReportService reportService, HttpClient httpClient, Microsoft.Extensions.Options.IOptions<SupabaseSettings> options)
    {
        _reportService = reportService;
        _httpClient = httpClient;
        _settings = options.Value;
    }

    [HttpGet]
    public IActionResult Index(ReportFilterViewModel model)
    {
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(ReportFilterViewModel model, CancellationToken cancellationToken)
    {
        var rows = await FetchRowsAsync(model.ReportType, model.StartDate, model.EndDate, cancellationToken);
        ViewBag.Rows = rows;
        ViewBag.ReportType = model.ReportType;
        ViewBag.StartDate = model.StartDate;
        ViewBag.EndDate = model.EndDate;
        return View("Preview", model);
    }

    [HttpGet]
    public async Task<IActionResult> Export(string type, string reportType, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        var rows = await FetchRowsAsync(reportType, startDate, endDate, cancellationToken);

        if (type == "excel")
        {
            var stream = await _reportService.GenerateExcelAsync(reportType, rows, cancellationToken);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reportType.ToLower()}-report.xlsx");
        }

        if (type == "csv")
        {
            var csv = _reportService.BuildCsv(rows);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"{reportType.ToLower()}-report.csv");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Print(string reportType, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        var rows = await FetchRowsAsync(reportType, startDate, endDate, cancellationToken);
        ViewBag.Rows = rows;
        ViewBag.ReportType = reportType;
        return View("Print", rows);
    }

    private async Task<List<Dictionary<string, object?>>> FetchRowsAsync(string reportType, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.Url) || string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            return new List<Dictionary<string, object?>>();
        }

        var table = reportType.ToLowerInvariant() switch
        {
            "assets" => "assets",
            "employees" => "profiles",
            "tickets" => "tickets",
            "departments" => "departments",
            "maintenance" => "maintenance",
            "technician performance" => "tickets",
            _ => "assets"
        };

        var query = new List<string> { "select=*" };
        if (startDate is not null)
        {
            query.Add($"created_at=gte.{startDate:yyyy-MM-dd}");
        }
        if (endDate is not null)
        {
            query.Add($"created_at=lte.{endDate:yyyy-MM-dd}");
        }

        var endpoint = $"{_settings.Url.TrimEnd('/')}/rest/v1/{table}?{string.Join('&', query)}";
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
        _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new List<Dictionary<string, object?>>();
        }

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        var data = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, System.Text.Json.JsonElement>>>(payload, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Dictionary<string, System.Text.Json.JsonElement>>();
        return data.Select(item => item.ToDictionary(kvp => kvp.Key, kvp => (object?)(kvp.Value.ValueKind == System.Text.Json.JsonValueKind.String ? kvp.Value.GetString() : kvp.Value.ToString()))).ToList();
    }
}
