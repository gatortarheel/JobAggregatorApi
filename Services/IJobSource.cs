using JobAggregatorApi.Models;

namespace JobAggregatorApi.Services;

public interface IJobSource
{
    string SourceName { get; }
    Task<IReadOnlyList<JobListing>> SearchAsync(
        JobSearchQuery query, CancellationToken ct = default);
}

/* 
Claude Opus 4.6 comments
This is the contract that every job source will implement. 
It's deliberately minimal -- just a name and a search method. 
The CancellationToken parameter is there so that if someone cancels an HTTP request to your API, that cancellation propagates down to the outbound HTTP calls to 
Adzuna, USAJobs, etc. rather than letting them run to completion for nothing.
IReadOnlyList<JobListing> rather than List<JobListing> because callers shouldn't be mutating the results. The source hands back data, the aggregator merges it -- clean separation.
The reason this matters architecturally: when you register multiple IJobSource implementations in DI, 
you can inject IEnumerable<IJobSource> into the aggregator and it automatically gets all of them. Adding a new job board later means writing one class and one DI registration line 
-- nothing else changes.
*/