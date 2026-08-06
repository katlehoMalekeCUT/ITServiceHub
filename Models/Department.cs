using System.Text.Json.Serialization;

namespace ServiceHub_IT.Models;

public class Department
{
    public string? id { get; set; }

    private string? _name;

    [JsonPropertyName("name")]
    public string name
    {
        get => _name ?? string.Empty;
        set => _name = value;
    }

    [JsonPropertyName("department_name")]
    public string? department_name
    {
        get => _name;
        set => _name = value;
    }

    public string? description { get; set; }
    public int? employee_count { get; set; }
    public int? asset_count { get; set; }
    public DateTime? created_at { get; set; } = DateTime.UtcNow;
    public DateTime? updated_at { get; set; } = DateTime.UtcNow;
}
