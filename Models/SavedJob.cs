namespace JobAggregatorApi.Models;

public class SavedJob
{
    public int Id { get; set; }
    public string SourceId { get; set; } = "";
    public string Source { get; set; } = "";
    public string Title { get; set; } = "";
    public string Company { get; set; } = "";
    public string Location { get; set; } = "";
    public string Description { get; set; } = "";
    public string Url { get; set; } = "";
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string? Category { get; set; }
    public DateTime PostedDate { get; set; }
    public DateTime FirstSeenAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

    public string? Status { get; set; }

    public int? MatchScore { get; set; }
    public string? MatchRationale { get; set; }
}