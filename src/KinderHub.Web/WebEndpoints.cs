using System.Text.Json;
using KinderHub.Web.Realtime;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KinderHub.Web;

// Extra endpoints of the app (besides the Blazor pages)
public static class WebEndpoints
{
    public static void MapKinderHubEndpoints(WebApplication app)
    {
        // SignalR hub for live notifications
        app.MapHub<NotificationHub>("/hubs/notifications");

        // Health check: open http://localhost:5161/health
        app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = WriteHealthJsonAsync });
    }

    private static Task WriteHealthJsonAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var body = new
        {
            status = report.Status.ToString(),
            durationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 1),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                data = e.Value.Data
            })
        };
        return context.Response.WriteAsync(JsonSerializer.Serialize(body, new JsonSerializerOptions { WriteIndented = true }));
    }
}
