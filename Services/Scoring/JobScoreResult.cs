namespace JobAggregatorApi.Services.Scoring;

public record JobScoreResult(
    int Score,
    string Rationale
);