using KinderHub.Application.Interfaces;
using KinderHub.Infrastructure.Persistence;
using KinderHub.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace KinderHub.Tests.Helpers;

// A real SQLite database that lives only in memory, created fresh for every test.
// Same database engine as the app → tests check real SQL, constraints and relations.
public sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public IDbContextFactory<KinderHubDbContext> Factory { get; }
    public FakeNotifier Notifier { get; } = new();
    public IMemoryCache Cache { get; } = new MemoryCache(new MemoryCacheOptions());

    public TestDatabase(bool seed = true)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<KinderHubDbContext>().UseSqlite(_connection).Options;
        Factory = new TestDbContextFactory(options);

        using var db = Factory.CreateDbContext();
        db.Database.EnsureCreated();
        if (seed)
        {
            DbSeeder.SeedAsync(db).GetAwaiter().GetResult();
        }
    }

    public ChildService Children() => new(Factory, Cache, NullLogger<ChildService>.Instance);
    public GroupService Groups() => new(Factory, Cache, NullLogger<GroupService>.Instance);
    public AttendanceService Attendance() => new(Factory, Notifier, NullLogger<AttendanceService>.Instance);
    public DailyReportService Reports() => new(Factory, Notifier, NullLogger<DailyReportService>.Instance);
    public AnnouncementService Announcements() => new(Factory, Notifier, NullLogger<AnnouncementService>.Instance);
    public DashboardService Dashboard() => new(Factory);

    public async Task<int> GroupIdAsync(string name) =>
        (await Groups().GetAllAsync()).Single(g => g.Name == name).Id;

    public async Task<int> ChildIdAsync(string firstName)
    {
        await using var db = await Factory.CreateDbContextAsync();
        return await db.Children.Where(c => c.FirstName == firstName).Select(c => c.Id).SingleAsync();
    }

    public void Dispose()
    {
        Cache.Dispose();
        _connection.Dispose();
    }

    private sealed class TestDbContextFactory(DbContextOptions<KinderHubDbContext> options) : IDbContextFactory<KinderHubDbContext>
    {
        public KinderHubDbContext CreateDbContext() => new(options);
    }
}

// Records the live events instead of sending them, so tests can check them
public sealed class FakeNotifier : IRealtimeNotifier
{
    public List<AttendanceChangedEvent> AttendanceEvents { get; } = new();
    public List<ReportSavedEvent> ReportEvents { get; } = new();
    public List<AnnouncementEvent> AnnouncementEvents { get; } = new();

#pragma warning disable CS0067 // events are part of the interface, tests don't subscribe
    public event Action<AttendanceChangedEvent>? AttendanceChanged;
    public event Action<ReportSavedEvent>? ReportSaved;
    public event Action<AnnouncementEvent>? AnnouncementChanged;
#pragma warning restore CS0067

    public Task PublishAsync(AttendanceChangedEvent e) { AttendanceEvents.Add(e); return Task.CompletedTask; }
    public Task PublishAsync(ReportSavedEvent e) { ReportEvents.Add(e); return Task.CompletedTask; }
    public Task PublishAsync(AnnouncementEvent e) { AnnouncementEvents.Add(e); return Task.CompletedTask; }
}
