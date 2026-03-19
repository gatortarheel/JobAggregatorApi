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

// Standard Services
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Job Sources & Aggregator
builder.Services.AddHttpClient<AdzunaJobSource>();
builder.Services.AddSingleton<IJobSource, AdzunaJobSource>();
builder.Services.AddHttpClient<UsaJobsSource>();
builder.Services.AddSingleton<IJobSource, UsaJobsSource>();
builder.Services.AddHttpClient<JoobleJobSource>();
builder.Services.AddSingleton<IJobSource, JoobleJobSource>();
builder.Services.AddSingleton<JobAggregatorService>();

// Database Configuration
// This looks for "ConnectionStrings:DefaultConnection" in your Azure App Settings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Data Source=jobs.db";

builder.Services.AddDbContext<JobDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<JobStorageService>();
builder.Services.AddHttpClient<JobScoringService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// NOTE: app.UseHttpsRedirection() is omitted here as Azure handles SSL termination
app.UseAuthorization();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapControllers();

// INITIALIZATION BLOCK
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 1. Ensure the /home directory exists for the SQLite file
        var sb = new SqliteConnectionStringBuilder(connectionString);
        var dbPath = sb.DataSource;
        var dbDir = Path.GetDirectoryName(dbPath);

        if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
        {
            Directory.CreateDirectory(dbDir);
        }

        // 2. Use EnsureCreated instead of Migrate for the POC
        // This builds the DB from your C# classes since Migrations folder is missing
        var db = services.GetRequiredService<JobDbContext>();
        db.Database.EnsureCreated();

        Console.WriteLine($"Database initialized successfully at: {dbPath}");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

app.Run();