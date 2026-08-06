using System.ComponentModel.DataAnnotations;

namespace ServiceHub_IT.Models;

public class AssetFormViewModel
{
    public string? Id { get; set; }

    [Required]
    public string AssetTag { get; set; } = string.Empty;

    [Required]
    public string AssetName { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime? PurchaseDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? WarrantyExpiry { get; set; }

    public string Department { get; set; } = string.Empty;
    public string AssignedEmployee { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = "Available";

    public IFormFile? ImageFile { get; set; }
    public IFormFile? WarrantyFile { get; set; }
}

public class AssetSearchViewModel
{
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
