using JobAggregatorApi.Models;
using Microsoft.EntityFrameworkCore;

namespace JobAggregatorApi.Data;

public class JobDbContext : DbContext
{
    public JobDbContext(DbContextOptions<JobDbContext> options) : base(options) { }

    public DbSet<SavedJob> SavedJobs => Set<SavedJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SavedJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Source, e.SourceId }).IsUnique();
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.PostedDate);
        });
    }
}