using JobAggregatorApi.Services;
using JobAggregatorApi.Services.Adzuna;
using JobAggregatorApi.Services.UsaJobs;
using Scalar.AspNetCore;
using JobAggregatorApi.Data;
using Microsoft.EntityFrameworkCore;
using JobAggregatorApi.Services.Scoring;
using JobAggregatorApi.Services.Jooble;
using JobAggregatorApi.Components;

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
builder.Services.AddDbContext<JobDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")
        ?? "Data Source=jobs.db"));
// Storage
builder.Services.AddScoped<JobStorageService>();

builder.Services.AddHttpClient<JobScoringService>();
builder.Services.AddScoped<JobScoringService>();

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
    var db = scope.ServiceProvider.GetRequiredService<JobDbContext>();
    db.Database.Migrate();
}

app.Run();