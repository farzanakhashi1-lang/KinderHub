using KinderHub.Application.DTOs;
using KinderHub.Application.Interfaces;
using KinderHub.Domain.Enums;
using KinderHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KinderHub.Infrastructure.Services;

public class DashboardService(IDbContextFactory<KinderHubDbContext> dbFactory) : IDashboardService
{
    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var from = today.AddDays(-13);

        var totalChildren = await db.Children.CountAsync(ct);
        var totalGroups = await db.Groups.CountAsync(ct);

        // Load only Date + Status for the last 14 days (small and fast)
        var history = await db.AttendanceRecords.AsNoTracking()
            .Where(a => a.Date >= from && a.Date <= today)
            .Select(a => new { a.Date, a.Status })
            .ToListAsync(ct);

        static bool CameIn(AttendanceStatus s) => s is AttendanceStatus.Present or AttendanceStatus.Late;

        var todays = history.Where(h => h.Date == today).ToList();
        var presentToday = todays.Count(h => CameIn(h.Status));
        var absentToday = todays.Count - presentToday;
        var notMarked = Math.Max(0, totalChildren - todays.Count);

        var rate = history.Count == 0 ? 0 : Math.Round(100.0 * history.Count(h => CameIn(h.Status)) / history.Count, 1);

        var trend = history
            .GroupBy(h => h.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailyAttendancePoint(g.Key, g.Count(x => CameIn(x.Status)), g.Count(x => !CameIn(x.Status))))
            .ToList();

        var occupancy = await db.Groups.AsNoTracking()
            .OrderBy(g => g.Name)
            .Select(g => new GroupOccupancy(g.Name, g.Children.Count, g.Capacity))
            .ToListAsync(ct);

        return new DashboardStatsDto(totalChildren, totalGroups, presentToday, absentToday, notMarked, rate, trend, occupancy);
    }
}
