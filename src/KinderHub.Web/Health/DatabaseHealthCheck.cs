using KinderHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KinderHub.Web.Health;

// Health check: "Is the database reachable?" Used by /health (monitoring, deployment checks).
public class DatabaseHealthCheck(IDbContextFactory<KinderHubDbContext> dbFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
            if (!await db.Database.CanConnectAsync(cancellationToken))
                return HealthCheckResult.Unhealthy("Cannot connect to the database.");

            var children = await db.Children.CountAsync(cancellationToken);
            return HealthCheckResult.Healthy("Database is reachable.",
                new Dictionary<string, object> { ["children"] = children });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database check failed.", ex);
        }
    }
}
