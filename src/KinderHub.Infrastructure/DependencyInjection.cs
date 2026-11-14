using KinderHub.Application.Interfaces;
using KinderHub.Infrastructure.Persistence;
using KinderHub.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KinderHub.Infrastructure;

// One place that registers everything from the Infrastructure layer.
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // DbContextFactory is the recommended way for Blazor Server.
        // The retry strategy makes saving resilient to short "database busy" moments.
        services.AddDbContextFactory<KinderHubDbContext>(options =>
            options.UseSqlite(connectionString, sqlite =>
                sqlite.ExecutionStrategy(deps => new SqliteRetryStrategy(deps))));

        // In-memory cache for data that is read often and changes rarely (groups)
        services.AddMemoryCache();

        // Services: pages ask for the INTERFACE, they get this implementation
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IChildService, ChildService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IDailyReportService, DailyReportService>();
        services.AddScoped<IAnnouncementService, AnnouncementService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }

    // Creates the database (if it does not exist) and adds demo data at startup
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<KinderHubDbContext>>();
        await using var db = await factory.CreateDbContextAsync();
        await db.Database.EnsureCreatedAsync();
        await DbSeeder.SeedAsync(db);
    }
}
