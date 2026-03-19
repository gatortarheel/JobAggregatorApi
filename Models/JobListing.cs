namespace JobAggregatorApi.Models;

public record JobListing
{
    public string SourceId { get; init; } = "";
    public string Source { get; init; } = "";
    public string Title { get; init; } = "";
    public string Company { get; init; } = "";
    public string Location { get; init; } = "";
    public string Description { get; init; } = "";
    public string Url { get; init; } = "";
    public decimal? SalaryMin { get; init; }
    public decimal? SalaryMax { get; init; }
    public string? Category { get; init; }
    public DateTime PostedDate { get; init; }
    public DateTime RetrievedAt { get; init; } = DateTime.UtcNow;
}