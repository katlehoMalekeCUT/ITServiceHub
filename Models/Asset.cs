namespace ServiceHub_IT.Models;

public class Asset
{
    public string? id { get; set; }
    public string asset_tag { get; set; } = string.Empty;
    public string asset_name { get; set; } = string.Empty;
    public string brand { get; set; } = string.Empty;
    public string model { get; set; } = string.Empty;
    public string serial_number { get; set; } = string.Empty;
    public string category { get; set; } = string.Empty;
    public DateTime? purchase_date { get; set; }
    public DateTime? warranty_expiry { get; set; }
    public string department { get; set; } = string.Empty;
    public string assigned_employee { get; set; } = string.Empty;
    public string location { get; set; } = string.Empty;
    public string status { get; set; } = "Available";
    public string? image_url { get; set; }
    public string? warranty_url { get; set; }
    public string? qr_code_url { get; set; }
}
