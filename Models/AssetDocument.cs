namespace ServiceHub_IT.Models;

public class AssetDocument
{
    public string? id { get; set; }
    public string asset_id { get; set; } = string.Empty;
    public string document_type { get; set; } = string.Empty;
    public string original_name { get; set; } = string.Empty;
    public string storage_path { get; set; } = string.Empty;
    public string storage_url { get; set; } = string.Empty;
    public string? extracted_text { get; set; }
    public DateTime? created_at { get; set; } = DateTime.UtcNow;
}
