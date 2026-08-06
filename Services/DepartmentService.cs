using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ServiceHub_IT.Models;

namespace ServiceHub_IT.Services;

public class DepartmentService
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseSettings _settings;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(HttpClient httpClient, IOptions<SupabaseSettings> options, ILogger<DepartmentService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Department>> GetDepartmentsAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Url) || string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            return Array.Empty<Department>();
        }

        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.GetAsync($"{_settings.Url.TrimEnd('/')}/rest/v1/departments?select=*", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Array.Empty<Department>();
            }

            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<List<Department>>(payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Department>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to load departments from Supabase.");
            return Array.Empty<Department>();
        }
    }

    public async Task<Department?> GetDepartmentAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.GetAsync($"{_settings.Url.TrimEnd('/')}/rest/v1/departments?id=eq.{id}&select=*", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<List<Department>>(payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to load department from Supabase.");
            return null;
        }
    }

    public async Task<bool> CreateDepartmentAsync(Department department, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(department);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.PostAsync($"{_settings.Url.TrimEnd('/')}/rest/v1/departments", content, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to create department in Supabase.");
            return false;
        }
    }

    public async Task<bool> UpdateDepartmentAsync(Department department, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(department);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.PatchAsync($"{_settings.Url.TrimEnd('/')}/rest/v1/departments?id=eq.{department.id}", content, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to update department in Supabase.");
            return false;
        }
    }

    public async Task<bool> DeleteDepartmentAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.DeleteAsync($"{_settings.Url.TrimEnd('/')}/rest/v1/departments?id=eq.{id}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to delete department from Supabase.");
            return false;
        }
    }
}
