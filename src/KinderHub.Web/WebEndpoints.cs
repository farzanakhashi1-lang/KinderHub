using System.Text.Json;
using KinderHub.Application.Common;
using KinderHub.Application.Interfaces;
using KinderHub.Web.Realtime;
using KinderHub.Web.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KinderHub.Web;

// Extra endpoints of the app (besides the Blazor pages)
public static class WebEndpoints
{
    public static void MapKinderHubEndpoints(WebApplication app)
    {
        // API documentation (OpenAPI): http://localhost:5161/openapi/v1.json
        app.MapOpenApi();

        // SignalR hub for live notifications
        app.MapHub<NotificationHub>("/hubs/notifications");

        // Health check: open http://localhost:5161/health
        app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = WriteHealthJsonAsync })
            .WithTags("Monitoring")
            .WithDisplayName("Health check");

        // CSV export of attendance (only Admin and Teacher)
        app.MapGet("/export/attendance.csv", async (int? groupId, IAttendanceService attendance, CancellationToken ct) =>
            {
                var page = await attendance.GetHistoryAsync(groupId, 0, 10_000, ct);
                var file = CsvExporter.Attendance(page.Items);
                return Results.File(file, "text/csv; charset=utf-8", $"attendance-{DateTime.Today:yyyy-MM-dd}.csv");
            })
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.Teacher))
            .WithName("ExportAttendanceCsv")
            .WithTags("Export")
            .WithSummary("Download attendance as CSV")
            .WithDescription("Admin or Teacher only. Optional groupId filters one group. UTF-8 with BOM, protected against CSV injection.")
            .Produces(StatusCodes.Status200OK, contentType: "text/csv");
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
