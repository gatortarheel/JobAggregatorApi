using JobAggregatorApi.Models;

namespace JobAggregatorApi.Services;

public static class SearchProfiles
{
    public static IReadOnlyList<JobSearchQuery> SeniorDotNetEngineer()
    {
        var locations = new[] { "Richmond, VA", "Virginia" };
        var radiusMiles = 50;
        var maxResults = 50;

        var keywords = new[]
        {
            // Direct .NET titles
            ".NET developer",
            ".NET engineer",
            "dotnet developer",
            "dotnet engineer",

            // C# focused
            "C# developer",
            "C# engineer",

            // Senior/Lead variations
            "senior software engineer",
            "lead software engineer",
            "principal software engineer",
            "staff software engineer",

            // Full stack with .NET flavor
            "full stack .NET",
            "full stack C#",

            // DevOps hybrid roles you'd qualify for
            "DevOps engineer",
            "platform engineer",

            // Architect level
            "software architect",
            "solutions architect",

            // Government/public sector titles
            "IT specialist",
            "computer scientist",
            "application developer"
        };

        return keywords
            .SelectMany(kw => locations.Select(loc =>
                new JobSearchQuery(kw, loc, radiusMiles, maxResults)))
            .ToList();
    }
}