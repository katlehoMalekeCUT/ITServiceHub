using System.Text.Json.Serialization;

namespace ServiceHub_IT.Models;

public class Maintenance
{
    [JsonPropertyName("id")]
    public Guid id { get; set; }

    [JsonPropertyName("asset_id")]
    public Guid asset_id { get; set; }

    [JsonPropertyName("technician_id")]
    public Guid technician_id { get; set; }

    [JsonPropertyName("problem")]
    public string problem { get; set; } = string.Empty;

    [JsonPropertyName("solution")]
    public string? solution { get; set; }

    [JsonPropertyName("maintenance_date")]
    public DateTime? maintenance_date { get; set; }

    [JsonPropertyName("next_service_date")]
    public DateTime? next_service_date { get; set; }

    [JsonPropertyName("maintenance_cost")]
    public decimal? maintenance_cost { get; set; }

    [JsonPropertyName("status")]
    public string status { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime? created_at { get; set; } = DateTime.UtcNow;
}
