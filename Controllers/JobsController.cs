using JobAggregatorApi.Models;
using JobAggregatorApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobAggregatorApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly JobAggregatorService _aggregator;
    private readonly JobStorageService _storage;

    public JobsController(JobAggregatorService aggregator, JobStorageService storage)
    {
        _aggregator = aggregator;
        _storage = storage;
    }

    [HttpPost("saved/score")]
    public async Task<ActionResult> ScoreUnscored(CancellationToken ct = default)
    {
        var scored = await _storage.ScoreUnscoredJobsAsync(ct);
        return Ok(new
        {
            JobsScored = scored
        });
    }


    [HttpPost("search/profile/senior-dotnet")]
    public async Task<ActionResult> RunSeniorDotNetProfile(
         [FromQuery] bool save = true,
         CancellationToken ct = default)
    {
        var queries = SearchProfiles.SeniorDotNetEngineer();
        var allResults = new List<JobListing>();

        foreach (var query in queries)
        {
            var results = await _aggregator.SearchAllAsync(query, ct);
            allResults.AddRange(results);
        }

        var deduplicated = JobDeduplicator.Deduplicate(allResults);

        int? newJobs = null;
        int? jobsScored = null;

        if (save)
        {
            newJobs = await _storage.SaveJobsAsync(deduplicated, ct);
            jobsScored = await _storage.ScoreUnscoredJobsAsync(ct);
        }

        return Ok(new
        {
            QueriesRun = queries.Count,
            TotalRaw = allResults.Count,
            AfterDedup = deduplicated.Count,
            NewJobsSaved = newJobs,
            JobsScored = jobsScored,
            Results = deduplicated
        });
    }

    [HttpGet("search")]
    public async Task<ActionResult> Search(
        [FromQuery] string keywords,
        [FromQuery] string location,
        [FromQuery] int radiusMiles = 25,
        [FromQuery] int maxResults = 50,
        [FromQuery] bool save = false,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(keywords))
            return BadRequest("Keywords are required");

        if (string.IsNullOrWhiteSpace(location))
            return BadRequest("Location is required");

        var query = new JobSearchQuery(keywords, location, radiusMiles, maxResults);
        var results = await _aggregator.SearchAllAsync(query, ct);

        int? newJobs = null;
        if (save)
        {
            newJobs = await _storage.SaveJobsAsync(results, ct);
        }

        return Ok(new
        {
            Count = results.Count,
            NewJobsSaved = newJobs,
            Query = query,
            Results = results
        });
    }

    [HttpGet("saved")]
    public async Task<ActionResult> GetSaved(
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default)
    {
        var jobs = await _storage.GetSavedJobsAsync(keyword, page, pageSize, ct);
        return Ok(new
        {
            Count = jobs.Count,
            Page = page,
            PageSize = pageSize,
            Results = jobs
        });
    }
}