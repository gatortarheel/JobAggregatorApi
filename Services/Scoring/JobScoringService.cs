using System.Text;
using System.Text.Json;
using JobAggregatorApi.Models;

namespace JobAggregatorApi.Services.Scoring;

public class JobScoringService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<JobScoringService> _logger;

    private const string SystemPrompt = """
        You are a job matching assistant. You will receive a candidate's resume and a job description.
        Score the match from 1 to 5:
        
        5 - Definitely apply. Strong match on most requirements, candidate is well qualified.
        4 - Likely worth applying. Good match with minor gaps that are common/acceptable.
        3 - Borderline. Some relevant skills but significant gaps or misalignment.
        2 - Weak match. Few overlapping skills or major missing requirements.
        1 - Don't bother. Fundamentally different role or requirements.
        
        Consider: required skills match, seniority alignment, technology stack overlap,
        and whether gaps are learnable vs fundamental.
        
        Respond ONLY with JSON in this exact format, no markdown, no backticks:
        {"score": 5, "rationale": "Brief 1-2 sentence explanation"}
        """;

    private const string Resume = """
        Senior .NET / DevOps Engineer and Technical Lead with 15+ years of experience.
        
        Core Skills:
        - C#/.NET (Framework and Core), ASP.NET Core Web API, Blazor
        - Azure (DevOps, SignalR, Document Intelligence, AKS, SQL Server)
        - AWS (Bedrock, Lambda, SQS, Aurora PostgreSQL, Textract, S3)
        - SQL Server, PostgreSQL, Cosmos DB, Entity Framework Core
        - Angular, React, Vue/JavaScript frontend experience
        - Docker, OpenTofu/Terraform, CI/CD pipelines
        - Python (FastAPI, SQLAlchemy) for AI/ML integration work
        - AI/ML: AWS Bedrock, Strands Agents, vector databases, document processing
        
        Recent Experience:
        - Led AI platform migration at Softdocs: Azure Document Intelligence to AWS Bedrock
          to Strands Agents, building POCs and managing production cutovers
        - Built transcript extraction API using AWS Textract + Bedrock/Claude with
          multi-column layout detection and spatial coordinate mapping
        - Built supporting infrastructure: SQS FIFO queues, Lambda, Aurora PostgreSQL,
          LocalStack for local testing, OpenTofu for IaC
        - Blazor application for document review with PDF.js integration
        
        Prior Experience:
        - CapTech Consulting: AWS IoT Greengrass, Amazon Rekognition
        - Cedar Fair: New Relic monitoring, enterprise system modernization
        - Chesterfield County: Government IT systems
        - Full stack development across multiple frameworks and platforms
        
        Education & Recognition:
        - Governor's Award recipient
        - Active in TDD practices with xUnit and integration testing
        """;

    public JobScoringService(HttpClient http, IConfiguration config, ILogger<JobScoringService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public bool ShouldScore(JobListing job)
    {
        var text = $"{job.Title} {job.Description}".ToLowerInvariant();
        return text.Contains(".net") || text.Contains("c#") || text.Contains("csharp");
    }

    public async Task<JobScoreResult?> ScoreJobAsync(JobListing job, CancellationToken ct = default)
    {
        try
        {
            var apiKey = _config["Anthropic:ApiKey"];

            var prompt = $"""
                CANDIDATE RESUME:
                {Resume}
                
                JOB LISTING:
                Title: {job.Title}
                Company: {job.Company}
                Location: {job.Location}
                Description: {job.Description}
                Salary: {(job.SalaryMin.HasValue ? $"${job.SalaryMin:N0} - ${job.SalaryMax:N0}" : "Not listed")}
                """;

            var requestBody = new
            {
                model = "claude-haiku-4-5-20251001",
                max_tokens = 256,
                system = SystemPrompt,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            request.Headers.Add("x-api-key", apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");

            using var response = await _http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(ct);
            var responseJson = await JsonSerializer.DeserializeAsync<JsonElement>(stream, cancellationToken: ct);

            var responseText = responseJson
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? "";

            responseText = responseText
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var result = JsonSerializer.Deserialize<JsonElement>(responseText);
            var score = result.GetProperty("score").GetInt32();
            var rationale = result.GetProperty("rationale").GetString() ?? "";

            _logger.LogInformation(
                "Scored {Title} at {Company}: {Score}/5 - {Rationale}",
                job.Title, job.Company, score, rationale);

            return new JobScoreResult(score, rationale);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to score {Title} at {Company}",
                job.Title, job.Company);
            return null;
        }
    }


}