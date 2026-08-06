using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ServiceHub_IT.Models;

namespace ServiceHub_IT.Services;

public class TicketService
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseSettings _settings;
    private readonly ILogger<TicketService> _logger;

    public TicketService(HttpClient httpClient, IOptions<SupabaseSettings> options, ILogger<TicketService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Ticket>> GetTicketsAsync(string? requester = null, string? assignedTechnician = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Url) || string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            return Array.Empty<Ticket>();
        }

        try
        {
            var query = new List<string> { "select=*" };
            if (!string.IsNullOrWhiteSpace(requester))
            {
                query.Add($"requester=eq.{Uri.EscapeDataString(requester)}");
            }
            if (!string.IsNullOrWhiteSpace(assignedTechnician))
            {
                query.Add($"assigned_technician=eq.{Uri.EscapeDataString(assignedTechnician)}");
            }

            var endpoint = $"{_settings.Url.TrimEnd('/')}/rest/v1/tickets?{string.Join('&', query)}";
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Array.Empty<Ticket>();
            }

            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<List<Ticket>>(payload, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Ticket>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to load tickets from Supabase.");
            return Array.Empty<Ticket>();
        }
    }

    public async Task<Ticket?> GetTicketAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.GetAsync($"{_settings.Url.TrimEnd('/')}/rest/v1/tickets?id=eq.{id}&select=*", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<List<Ticket>>(payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to load ticket from Supabase.");
            return null;
        }
    }

    public async Task<bool> CreateTicketAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(ticket);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.PostAsync($"{_settings.Url.TrimEnd('/')}/rest/v1/tickets", content, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to create ticket in Supabase.");
            return false;
        }
    }

    public async Task<bool> UpdateTicketAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(ticket);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.PatchAsync($"{_settings.Url.TrimEnd('/')}/rest/v1/tickets?id=eq.{ticket.id}", content, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to update ticket in Supabase.");
            return false;
        }
    }

    public string GetScreenshotBucketName()
    {
        return string.IsNullOrWhiteSpace(_settings.TicketScreenshotBucketName)
            ? "tickets-screenshots"
            : _settings.TicketScreenshotBucketName;
    }

    public async Task<string?> UploadScreenshotAsync(IFormFile file, string path, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        var bucketName = GetScreenshotBucketName();

        try
        {
            var content = new StreamContent(file.OpenReadStream());
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Add("x-upsert", "true");

            using var response = await _httpClient.PostAsync($"{_settings.Url.TrimEnd('/')}/storage/v1/object/{bucketName}/{path}", content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return $"{_settings.Url.TrimEnd('/')}/storage/v1/object/public/{bucketName}/{path}";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to upload screenshot to Supabase Storage.");
            return null;
        }
    }
}
