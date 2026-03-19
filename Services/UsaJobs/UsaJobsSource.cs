using System.Text.Json;
using JobAggregatorApi.Models;

namespace JobAggregatorApi.Services.UsaJobs;

public class UsaJobsSource : IJobSource
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public string SourceName => "USAJobs";

    public UsaJobsSource(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<IReadOnlyList<JobListing>> SearchAsync(
        JobSearchQuery query, CancellationToken ct = default)
    {
        var apiKey = _config["UsaJobs:ApiKey"];
        var email = _config["UsaJobs:Email"];

        var url = $"https://data.usajobs.gov/api/Search" +
            $"?Keyword={Uri.EscapeDataString(query.Keywords)}" +
            $"&LocationName={Uri.EscapeDataString(query.Location)}" +
            $"&Radius={query.RadiusMiles}" +
            $"&ResultsPerPage={query.MaxResults}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation("Authorization-Key", apiKey);
        request.Headers.TryAddWithoutValidation("User-Agent", email);

        using var response = await _http.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        var data = await JsonSerializer.DeserializeAsync<UsaJobsResponse>(
            stream, cancellationToken: ct);

        return data?.SearchResult?.SearchResultItems?.Select(item =>
        {
            var job = item.MatchedObjectDescriptor;
            var remuneration = job.PositionRemuneration?.FirstOrDefault();

            return new JobListing
            {
                SourceId = item.MatchedObjectId,
                Source = SourceName,
                Title = job.PositionTitle,
                Company = job.OrganizationName,
                Location = job.PositionLocationDisplay,
                Description = job.QualificationSummary,
                Url = job.PositionUri,
                SalaryMin = decimal.TryParse(remuneration?.MinimumRange, out var min) ? min : null,
                SalaryMax = decimal.TryParse(remuneration?.MaximumRange, out var max) ? max : null,
                Category = job.JobCategory?.FirstOrDefault()?.Name,
                PostedDate = DateTime.Parse(job.PublicationStartDate)
            };
        }).ToList() ?? [];
    }
}