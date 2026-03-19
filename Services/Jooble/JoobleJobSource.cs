using System.Text;
using System.Text.Json;
using JobAggregatorApi.Models;

namespace JobAggregatorApi.Services.Jooble;

public class JoobleJobSource : IJobSource
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public string SourceName => "Jooble";

    public JoobleJobSource(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<IReadOnlyList<JobListing>> SearchAsync(
        JobSearchQuery query, CancellationToken ct = default)
    {
        var apiKey = _config["Jooble:ApiKey"];
        var url = $"https://jooble.org/api/{apiKey}";

        var requestBody = new
        {
            keywords = query.Keywords,
            location = query.Location,
            radius = query.RadiusMiles.ToString(),
            page = "1",
            ResultOnPage = query.MaxResults
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _http.PostAsync(url, content, ct);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        var data = await JsonSerializer.DeserializeAsync<JoobleResponse>(stream, cancellationToken: ct);

        return data?.Jobs?.Select(j => new JobListing
        {
            SourceId = j.Id.ToString(),
            Source = SourceName,
            Title = j.Title ?? "Untitled",
            Company = j.Company ?? "Unknown",
            Location = j.Location ?? "",
            Description = j.Snippet,
            Url = j.Link,
            SalaryMin = ParseSalaryMin(j.Salary),
            SalaryMax = ParseSalaryMax(j.Salary),
            Category = j.Type,
            PostedDate = DateTime.TryParse(j.Updated, out var dt) ? dt : DateTime.UtcNow
        }).ToList() ?? [];
    }

    private static decimal? ParseSalaryMin(string? salary)
    {
        if (string.IsNullOrWhiteSpace(salary)) return null;

        var cleaned = new string(salary.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());
        var parts = cleaned.Split('-');

        if (parts.Length > 0 && decimal.TryParse(parts[0].Trim(), out var min))
            return min;

        return null;
    }

    private static decimal? ParseSalaryMax(string? salary)
    {
        if (string.IsNullOrWhiteSpace(salary)) return null;

        var cleaned = new string(salary.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());
        var parts = cleaned.Split('-');

        if (parts.Length > 1 && decimal.TryParse(parts[1].Trim(), out var max))
            return max;

        return null;
    }
}