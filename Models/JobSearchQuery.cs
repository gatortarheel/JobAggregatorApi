namespace JobAggregatorApi.Models;


/*Using record types here gives you immutability and value equality out of the box, which is nice for data transfer objects like these.*/
public record JobSearchQuery(
    string Keywords,
    string Location,
    int RadiusMiles = 25,
    int MaxResults = 50
);