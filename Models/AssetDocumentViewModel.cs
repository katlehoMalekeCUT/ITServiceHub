namespace ServiceHub_IT.Models;

public class AssetDocumentUploadViewModel
{
    public string AssetId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = "Warranty";
    public IFormFile? DocumentFile { get; set; }
}
