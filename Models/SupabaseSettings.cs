namespace ServiceHub_IT.Models;

public sealed class SupabaseSettings
{
    public const string SectionName = "Supabase";

    public string Url { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
    public string TicketScreenshotBucketName { get; set; } = "tickets-screenshots";
    public string AssetDocumentBucketName { get; set; } = "asset-documents";
    public string AssetImageBucketName { get; set; } = "assets-images";
    public string AssetWarrantyBucketName { get; set; } = "assets-warranty";
}
