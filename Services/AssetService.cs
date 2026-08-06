using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ServiceHub_IT.Models;

namespace ServiceHub_IT.Services;

public class AssetService
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseSettings _settings;
    private readonly ILogger<AssetService> _logger;

    public AssetService(HttpClient httpClient, IOptions<SupabaseSettings> options, ILogger<AssetService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Asset>> GetAssetsAsync(AssetSearchViewModel search, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Url) || string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            return Array.Empty<Asset>();
        }

        try
        {
            var query = new List<string> { "select=*" };
            if (!string.IsNullOrWhiteSpace(search.SearchTerm))
            {
                query.Add($"asset_name=ilike.*{Uri.EscapeDataString(search.SearchTerm)}*");
            }
            if (!string.IsNullOrWhiteSpace(search.Category))
            {
                query.Add($"category=eq.{Uri.EscapeDataString(search.Category)}");
            }
            if (!string.IsNullOrWhiteSpace(search.Status))
            {
                query.Add($"status=eq.{Uri.EscapeDataString(search.Status)}");
            }

            var endpoint = $"{_settings.Url.TrimEnd('/')}/rest/v1/assets?{string.Join('&', query)}";
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Array.Empty<Asset>();
            }

            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<List<Asset>>(payload, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Asset>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to load assets from Supabase.");
            return Array.Empty<Asset>();
        }
    }

    public async Task<Asset?> GetAssetAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.GetAsync($"{_settings.Url.TrimEnd('/')}/rest/v1/assets?id=eq.{id}&select=*", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<List<Asset>>(payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to load asset from Supabase.");
            return null;
        }
    }

    public async Task<bool> CreateAssetAsync(Asset asset, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(asset);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.PostAsync($"{_settings.Url.TrimEnd('/')}/rest/v1/assets", content, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to create asset in Supabase.");
            return false;
        }
    }

    public async Task<bool> UpdateAssetAsync(Asset asset, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(asset);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.PatchAsync($"{_settings.Url.TrimEnd('/')}/rest/v1/assets?id=eq.{asset.id}", content, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to update asset in Supabase.");
            return false;
        }
    }

    public async Task<bool> DeleteAssetAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.DeleteAsync($"{_settings.Url.TrimEnd('/')}/rest/v1/assets?id=eq.{id}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to delete asset from Supabase.");
            return false;
        }
    }

    public string GetAssetImageBucketName()
    {
        return string.IsNullOrWhiteSpace(_settings.AssetImageBucketName)
            ? "assets-images"
            : _settings.AssetImageBucketName;
    }

    public string GetAssetWarrantyBucketName()
    {
        return string.IsNullOrWhiteSpace(_settings.AssetWarrantyBucketName)
            ? "assets-warranty"
            : _settings.AssetWarrantyBucketName;
    }

    public async Task<string?> UploadFileAsync(IFormFile? file, string bucket, string path, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        try
        {
            var content = new StreamContent(file.OpenReadStream());
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Add("x-upsert", "true");

            using var response = await _httpClient.PostAsync($"{_settings.Url.TrimEnd('/')}/storage/v1/object/{bucket}/{path}", content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return $"{_settings.Url.TrimEnd('/')}/storage/v1/object/public/{bucket}/{path}";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to upload file to Supabase Storage.");
            return null;
        }
    }

    public string GenerateQrCodeUrl(string assetTag)
    {
        return $"https://api.qrserver.com/v1/create?size=200x200&data={Uri.EscapeDataString(assetTag)}";
    }
}
