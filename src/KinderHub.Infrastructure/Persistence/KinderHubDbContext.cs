using KinderHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KinderHub.Infrastructure.Persistence;

// The bridge between our C# classes and the database tables.
public class KinderHubDbContext(DbContextOptions<KinderHubDbContext> options) : DbContext(options)
{
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Child> Children => Set<Child>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<DailyReport> DailyReports => Set<DailyReport>();
    public DbSet<Announcement> Announcements => Set<Announcement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Loads all the rules from the Configurations folder
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KinderHubDbContext).Assembly);
    }
}
