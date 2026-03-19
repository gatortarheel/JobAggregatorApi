using System.Text.Json.Serialization;

namespace JobAggregatorApi.Services.Adzuna;

public record AdzunaResponse(
    [property: JsonPropertyName("results")] List<AdzunaJob> Results);

public record AdzunaJob(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("redirect_url")] string RedirectUrl,
    [property: JsonPropertyName("created")] string Created,
    [property: JsonPropertyName("salary_min")] decimal? SalaryMin,
    [property: JsonPropertyName("salary_max")] decimal? SalaryMax,
    [property: JsonPropertyName("company")] AdzunaCompany? Company,
    [property: JsonPropertyName("location")] AdzunaLocation? Location,
    [property: JsonPropertyName("category")] AdzunaCategory? Category);

public record AdzunaCompany(
    [property: JsonPropertyName("display_name")] string DisplayName);

public record AdzunaLocation(
    [property: JsonPropertyName("display_name")] string DisplayName);

public record AdzunaCategory(
    [property: JsonPropertyName("label")] string Label);