using JobAggregatorApi.Models;

namespace JobAggregatorApi.Services;

public class JobAggregatorService
{
    private readonly IEnumerable<IJobSource> _sources;
    private readonly ILogger<JobAggregatorService> _logger;

    public JobAggregatorService(
        IEnumerable<IJobSource> sources,
        ILogger<JobAggregatorService> logger)
    {
        _sources = sources;
        _logger = logger;
    }

    public async Task<IReadOnlyList<JobListing>> SearchAllAsync(
        JobSearchQuery query, CancellationToken ct = default)
    {
        var tasks = _sources.Select(async source =>
        {
            try
            {
                var results = await source.SearchAsync(query, ct);
                _logger.LogInformation(
                    "Source {Source} returned {Count} results",
                    source.SourceName, results.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Source {Source} failed", source.SourceName);
                return Array.Empty<JobListing>() as IReadOnlyList<JobListing>;
            }
        });

        var results = await Task.WhenAll(tasks);

        return results
            .SelectMany(r => r)
            .OrderByDescending(j => j.PostedDate)
            .ToList();
    }
}