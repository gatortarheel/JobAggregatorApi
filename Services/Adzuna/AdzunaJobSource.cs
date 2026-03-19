using System.Text.Json;
using JobAggregatorApi.Models;

namespace JobAggregatorApi.Services.Adzuna;

public class AdzunaJobSource : IJobSource
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public string SourceName => "Adzuna";

    public AdzunaJobSource(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<IReadOnlyList<JobListing>> SearchAsync(
        JobSearchQuery query, CancellationToken ct = default)
    {
        var appId = _config["Adzuna:AppId"];
        var appKey = _config["Adzuna:AppKey"];
        var daysOld = 14;

        var location = query.Location
            .Split(',')[0]
            .Trim();

        var url = $"https://api.adzuna.com/v1/api/jobs/us/search/1" +
            $"?app_id={appId}" +
            $"&app_key={appKey}" +
            $"&results_per_page={query.MaxResults}" +
            $"&what={Uri.EscapeDataString(query.Keywords)}" +
            $"&where={Uri.EscapeDataString(location)}" +
            $"&distance={query.RadiusMiles}" +
            $"&max_days_old={daysOld}" +
            $"&content-type=application/json";

        // Use SendAsync + ReadAsStreamAsync to completely bypass
        // Adzuna's non-standard "utf8" charset header
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await _http.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        var data = await JsonSerializer.DeserializeAsync<AdzunaResponse>(
            stream, cancellationToken: ct);

        return data?.Results?.Select(r => new JobListing
        {
            SourceId = r.Id,
            Source = SourceName,
            Title = r.Title,
            Company = r.Company?.DisplayName ?? "Unknown",
            Location = r.Location?.DisplayName ?? "",
            Description = r.Description,
            Url = r.RedirectUrl,
            SalaryMin = r.SalaryMin,
            SalaryMax = r.SalaryMax,
            Category = r.Category?.Label,
            PostedDate = DateTime.Parse(r.Created)
        }).ToList() ?? [];
    }
}