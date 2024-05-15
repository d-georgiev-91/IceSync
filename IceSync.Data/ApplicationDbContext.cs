using IceSync.Data.Configurations;
using IceSync.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IceSync.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Workflow> Workflows { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new WorkflowConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}