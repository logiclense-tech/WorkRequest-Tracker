using Microsoft.EntityFrameworkCore;
using WorkRequestTracker.Api.Models;

namespace WorkRequestTracker.Api.Data;

public sealed class WorkRequestDbContext(DbContextOptions<WorkRequestDbContext> options)
    : DbContext(options)
{
    public DbSet<WorkRequest> WorkRequests => Set<WorkRequest>();
    public DbSet<WorkRequestNote> WorkRequestNotes => Set<WorkRequestNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkRequest>(entity =>
        {
            entity.ToTable("WorkRequests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ClientName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Priority).HasConversion<string>().HasMaxLength(10);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(x => new { x.Status, x.CreatedAtUtc });
            entity.HasIndex(x => x.ClientName);
            entity.HasIndex(x => x.Title);
            entity.HasMany(x => x.Notes).WithOne(x => x.WorkRequest)
                .HasForeignKey(x => x.WorkRequestId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<WorkRequestNote>(entity =>
        {
            entity.ToTable("WorkRequestNotes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Text).HasMaxLength(1000).IsRequired();
        });
    }
}
