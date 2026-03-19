using JobAggregatorApi.Data;
using JobAggregatorApi.Models;
using JobAggregatorApi.Services.Scoring;
using Microsoft.EntityFrameworkCore;

namespace JobAggregatorApi.Services;

public class JobStorageService
{
    private readonly JobDbContext _db;
    private readonly ILogger<JobStorageService> _logger;
    private readonly JobScoringService _scoring;

    public JobStorageService(JobDbContext db, ILogger<JobStorageService> logger, JobScoringService scoring)
    {
        _db = db;
        _scoring = scoring;
        _logger = logger;
    }

    public async Task<int> SaveJobsAsync(IReadOnlyList<JobListing> jobs, CancellationToken ct = default)
    {
        var newCount = 0;
        var savedJobs = await _db.SavedJobs.ToListAsync(ct);

        foreach (var job in jobs)
        {
            var existing = savedJobs
                .FirstOrDefault(j => j.Source == job.Source && j.SourceId == job.SourceId);

            if (existing is not null)
            {
                existing.LastSeenAt = DateTime.UtcNow;
                existing.Title = job.Title;
                existing.SalaryMin = job.SalaryMin;
                existing.SalaryMax = job.SalaryMax;
                continue;
            }

            var normalizedTitle = Normalize(job.Title);
            var normalizedCompany = Normalize(job.Company);

            var crossMatch = savedJobs
                .FirstOrDefault(j =>
                    Normalize(j.Title) == normalizedTitle &&
                    Normalize(j.Company) == normalizedCompany);

            if (crossMatch is not null)
            {
                crossMatch.LastSeenAt = DateTime.UtcNow;

                if (!crossMatch.SalaryMin.HasValue && job.SalaryMin.HasValue)
                    crossMatch.SalaryMin = job.SalaryMin;
                if (!crossMatch.SalaryMax.HasValue && job.SalaryMax.HasValue)
                    crossMatch.SalaryMax = job.SalaryMax;

                continue;
            }

            var newJob = new SavedJob
            {
                SourceId = job.SourceId,
                Source = job.Source,
                Title = job.Title,
                Company = job.Company,
                Location = job.Location,
                Description = job.Description,
                Url = job.Url,
                SalaryMin = job.SalaryMin,
                SalaryMax = job.SalaryMax,
                Category = job.Category,
                PostedDate = job.PostedDate
            };

            _db.SavedJobs.Add(newJob);
            savedJobs.Add(newJob);
            newCount++;
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Saved {New} new jobs, updated {Updated} existing",
            newCount, jobs.Count - newCount);

        return newCount;
    }

    public async Task<int> ScoreUnscoredJobsAsync(CancellationToken ct = default)
    {
        var unscored = await _db.SavedJobs
            .Where(j => j.MatchScore == null)
            .ToListAsync(ct);

        var scoredCount = 0;

        foreach (var job in unscored)
        {
            var listing = new JobListing
            {
                SourceId = job.SourceId,
                Source = job.Source,
                Title = job.Title,
                Company = job.Company,
                Location = job.Location,
                Description = job.Description,
                Url = job.Url,
                SalaryMin = job.SalaryMin,
                SalaryMax = job.SalaryMax,
                Category = job.Category,
                PostedDate = job.PostedDate
            };

            if (!_scoring.ShouldScore(listing))
                continue;

            var result = await _scoring.ScoreJobAsync(listing, ct);
            if (result is not null)
            {
                job.MatchScore = result.Score;
                job.MatchRationale = result.Rationale;
                job.Status = result.Score >= 4 ? "To Be Reviewed" : "Skipped";
                scoredCount++;
            }
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Scored {Count} jobs out of {Total} unscored",
            scoredCount, unscored.Count);

        return scoredCount;
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        return new string(value
            .ToLowerInvariant()
            .Where(c => char.IsLetterOrDigit(c) || c == ' ')
            .ToArray())
            .Trim();
    }

    public async Task<IReadOnlyList<SavedJob>> GetSavedJobsAsync(
        string? keyword = null,
        int page = 1,
        int pageSize = 25,
        CancellationToken ct = default)
    {
        var query = _db.SavedJobs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lower = keyword.ToLower();
            query = query.Where(j =>
                j.Title.ToLower().Contains(lower) ||
                j.Company.ToLower().Contains(lower));
        }

        return await query
            .OrderByDescending(j => j.PostedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }
}
