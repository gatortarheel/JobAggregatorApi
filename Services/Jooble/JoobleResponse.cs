using System.Text.Json.Serialization;

namespace JobAggregatorApi.Services.Jooble;

public record JoobleResponse(
    [property: JsonPropertyName("totalCount")] int TotalCount,
    [property: JsonPropertyName("jobs")] List<JoobleJob>? Jobs);

public record JoobleJob(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("location")] string Location,
    [property: JsonPropertyName("snippet")] string Snippet,
    [property: JsonPropertyName("salary")] string? Salary,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("link")] string Link,
    [property: JsonPropertyName("company")] string Company,
    [property: JsonPropertyName("updated")] string Updated);