namespace ServiceHub_IT.Models;

public class TicketPrediction
{
    public string Priority { get; set; } = string.Empty;
    public int EstimatedResolutionHours { get; set; }
    public string RecommendedTechnician { get; set; } = string.Empty;
}
