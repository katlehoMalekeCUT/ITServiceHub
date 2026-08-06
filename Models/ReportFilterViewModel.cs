namespace ServiceHub_IT.Models;

public class ReportFilterViewModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string ReportType { get; set; } = "Assets";
}
