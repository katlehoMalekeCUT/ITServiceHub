using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using Microsoft.Extensions.Options;
using ServiceHub_IT.Models;

namespace ServiceHub_IT.Services;

public class PdfProcessingService
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseSettings _settings;
    private readonly ILogger<PdfProcessingService> _logger;

    public PdfProcessingService(HttpClient httpClient, IOptions<SupabaseSettings> options, ILogger<PdfProcessingService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }

    public string GetAssetDocumentBucketName()
    {
        return string.IsNullOrWhiteSpace(_settings.AssetDocumentBucketName)
            ? "asset-documents"
            : _settings.AssetDocumentBucketName;
    }

    public async Task<(string? storageUrl, string? extractedText)> ProcessAndUploadAsync(IFormFile file, string assetId, string documentType, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0 || !file.FileName.ToLower().EndsWith(".pdf"))
        {
            return (null, null);
        }

        var bucketName = GetAssetDocumentBucketName();

        try
        {
            var path = $"{assetId}/{documentType.ToLowerInvariant()}/{Guid.NewGuid():N}_{file.FileName}";
            var content = new StreamContent(file.OpenReadStream());
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Add("x-upsert", "true");

            using var response = await _httpClient.PostAsync($"{_settings.Url.TrimEnd('/')}/storage/v1/object/{bucketName}/{path}", content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return (null, null);
            }

            var storageUrl = $"{_settings.Url.TrimEnd('/')}/storage/v1/object/public/{bucketName}/{path}";
            var extractedText = ExtractText(file.OpenReadStream());
            return (storageUrl, extractedText);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to process PDF document.");
            return (null, null);
        }
    }

    public async Task<bool> SaveDocumentMetadataAsync(AssetDocument document, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(document);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.PostAsync($"{_settings.Url.TrimEnd('/')}/rest/v1/asset_documents", content, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to save asset document metadata to Supabase.");
            return false;
        }
    }

    public async Task<IReadOnlyList<AssetDocument>> GetDocumentsAsync(string assetId, CancellationToken cancellationToken = default)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.GetAsync($"{_settings.Url.TrimEnd('/')}/rest/v1/asset_documents?asset_id=eq.{assetId}&select=*", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Array.Empty<AssetDocument>();
            }

            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            return System.Text.Json.JsonSerializer.Deserialize<List<AssetDocument>>(payload, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<AssetDocument>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to load asset documents from Supabase.");
            return Array.Empty<AssetDocument>();
        }
    }

    public string ExtractText(Stream stream)
    {
        try
        {
            using var document = PdfDocument.Open(stream);
            var builder = new StringBuilder();
            foreach (var page in document.GetPages())
            {
                builder.AppendLine(page.Text);
            }
            return builder.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to extract text from PDF.");
            return string.Empty;
        }
    }
}
