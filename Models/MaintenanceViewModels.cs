using System.ComponentModel.DataAnnotations;

namespace ServiceHub_IT.Models;

public class MaintenanceFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    public Guid AssetId { get; set; }

    [Required]
    public Guid TechnicianId { get; set; }

    [Required]
    public string Problem { get; set; } = string.Empty;

    public string? Solution { get; set; }

    [DataType(DataType.Date)]
    public DateTime? MaintenanceDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? NextServiceDate { get; set; }

    [DataType(DataType.Currency)]
    public decimal? MaintenanceCost { get; set; }

    [Required]
    public string Status { get; set; } = "Open";

    public IEnumerable<Asset> Assets { get; set; } = Array.Empty<Asset>();
    public IEnumerable<Profile> Technicians { get; set; } = Array.Empty<Profile>();
}

public class MaintenanceRecordViewModel
{
    public Guid Id { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string TechnicianName { get; set; } = string.Empty;
    public string Problem { get; set; } = string.Empty;
    public string? Solution { get; set; }
    public DateTime? MaintenanceDate { get; set; }
    public DateTime? NextServiceDate { get; set; }
    public decimal? MaintenanceCost { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
}
