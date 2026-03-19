using JobAggregatorApi.Services;
using JobAggregatorApi.Services.Adzuna;
using JobAggregatorApi.Services.UsaJobs;
using Scalar.AspNetCore;
using JobAggregatorApi.Data;
using Microsoft.EntityFrameworkCore;
using JobAggregatorApi.Services.Scoring;
using JobAggregatorApi.Services.Jooble;
using JobAggregatorApi.Components;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register job sources
builder.Services.AddHttpClient<AdzunaJobSource>();
builder.Services.AddSingleton<IJobSource, AdzunaJobSource>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<UsaJobsSource>();
builder.Services.AddSingleton<IJobSource, UsaJobsSource>();

// Register aggregator
builder.Services.AddSingleton<JobAggregatorService>();

builder.Services.AddHttpClient<JoobleJobSource>();
builder.Services.AddSingleton<IJobSource, JoobleJobSource>();

// Database
// Change "Default" to "DefaultConnection"
builder.Services.AddDbContext<JobDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=jobs.db"));
// Storage
builder.Services.AddScoped<JobStorageService>();

builder.Services.AddHttpClient<JobScoringService>();

var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 1. Get the connection string (Matching the YAML name 'DefaultConnection')
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Data Source=jobs.db";

        // 2. Ensure the directory exists BEFORE EF Core touches it
        var sb = new SqliteConnectionStringBuilder(connectionString);
        var dbPath = sb.DataSource;
        var dbDir = Path.GetDirectoryName(dbPath);

        if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
        {
            Directory.CreateDirectory(dbDir);
        }

        // 3. Apply migrations (or use db.Database.EnsureCreated() if not using migrations)
        var db = services.GetRequiredService<JobDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

app.Run();