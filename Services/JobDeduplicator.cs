using JobAggregatorApi.Models;

namespace JobAggregatorApi.Services;

public static class JobDeduplicator
{
    public static IReadOnlyList<JobListing> Deduplicate(IReadOnlyList<JobListing> jobs)
    {
        var seen = new Dictionary<string, JobListing>();

        foreach (var job in jobs)
        {
            var key = GenerateKey(job);

            if (seen.TryGetValue(key, out var existing))
            {
                if (Score(job) > Score(existing))
                {
                    seen[key] = job;
                }
            }
            else
            {
                seen[key] = job;
            }
        }

        return seen.Values
            .OrderByDescending(j => j.PostedDate)
            .ToList();
    }

    private static string GenerateKey(JobListing job)
    {
        var title = Normalize(job.Title);
        var company = Normalize(job.Company);
        return $"{title}|{company}";
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

    private static int Score(JobListing job)
    {
        var score = 0;
        if (job.SalaryMin.HasValue) score += 2;
        if (job.SalaryMax.HasValue) score += 2;
        if (!string.IsNullOrWhiteSpace(job.Category)) score += 1;
        score += Math.Min(job.Description.Length / 100, 5);
        return score;
    }
}