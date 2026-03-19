using System.Text.Json.Serialization;

namespace JobAggregatorApi.Services.UsaJobs;

public record UsaJobsResponse(
    [property: JsonPropertyName("SearchResult")] UsaJobsSearchResult SearchResult);

public record UsaJobsSearchResult(
    [property: JsonPropertyName("SearchResultItems")] List<UsaJobsItem>? SearchResultItems,
    [property: JsonPropertyName("SearchResultCount")] int SearchResultCount,
    [property: JsonPropertyName("SearchResultCountAll")] int SearchResultCountAll);

public record UsaJobsItem(
    [property: JsonPropertyName("MatchedObjectId")] string MatchedObjectId,
    [property: JsonPropertyName("MatchedObjectDescriptor")] UsaJobsDescriptor MatchedObjectDescriptor);

public record UsaJobsDescriptor(
    [property: JsonPropertyName("PositionTitle")] string PositionTitle,
    [property: JsonPropertyName("OrganizationName")] string OrganizationName,
    [property: JsonPropertyName("PositionLocationDisplay")] string PositionLocationDisplay,
    [property: JsonPropertyName("QualificationSummary")] string QualificationSummary,
    [property: JsonPropertyName("PositionURI")] string PositionUri,
    [property: JsonPropertyName("PublicationStartDate")] string PublicationStartDate,
    [property: JsonPropertyName("PositionRemuneration")] List<UsaJobsRemuneration>? PositionRemuneration,
    [property: JsonPropertyName("JobCategory")] List<UsaJobsCategory>? JobCategory);

public record UsaJobsRemuneration(
    [property: JsonPropertyName("MinimumRange")] string MinimumRange,
    [property: JsonPropertyName("MaximumRange")] string MaximumRange,
    [property: JsonPropertyName("RateIntervalCode")] string? RateIntervalCode);

public record UsaJobsCategory(
    [property: JsonPropertyName("Name")] string Name,
    [property: JsonPropertyName("Code")] string Code);