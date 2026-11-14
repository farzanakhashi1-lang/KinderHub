using KinderHub.Application.Common;
using KinderHub.Application.DTOs;
using KinderHub.Application.Interfaces;
using KinderHub.Domain.Entities;
using KinderHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KinderHub.Infrastructure.Services;

public class DailyReportService(
    IDbContextFactory<KinderHubDbContext> dbFactory,
    IRealtimeNotifier notifier,
    ILogger<DailyReportService> logger) : IDailyReportService
{
    public async Task<IReadOnlyList<DailyReportDto>> GetForChildAsync(int childId, int take = 10, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.DailyReports.AsNoTracking()
            .Where(r => r.ChildId == childId)
            .OrderByDescending(r => r.Date)
            .Take(Math.Clamp(take, 1, 100))
            .Select(r => new DailyReportDto(r.Id, r.ChildId, r.Child.FirstName + " " + r.Child.LastName, r.Date,
                r.Mood, r.Meals, r.NapMinutes, r.Activities, r.TeacherComment, r.AuthorName))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DailyReportDto>> GetForGroupAndDateAsync(int groupId, DateOnly date, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.DailyReports.AsNoTracking()
            .Where(r => r.Child.GroupId == groupId && r.Date == date)
            .OrderBy(r => r.Child.FirstName)
            .Select(r => new DailyReportDto(r.Id, r.ChildId, r.Child.FirstName + " " + r.Child.LastName, r.Date,
                r.Mood, r.Meals, r.NapMinutes, r.Activities, r.TeacherComment, r.AuthorName))
            .ToListAsync(ct);
    }

    // Creates the report, or updates it if one already exists for this child and day
    public async Task<Result> SaveAsync(DailyReportEditModel model, string authorName, CancellationToken ct = default)
    {
        var validation = ModelValidator.Validate(model);
        if (!validation.Succeeded) return validation;

        var date = model.Date!.Value;

        // Same safe retry as attendance: one report per child per day, even if two teachers save together
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct);
            var child = await db.Children.AsNoTracking()
                .Where(c => c.Id == model.ChildId)
                .Select(c => new { c.GroupId, Name = c.FirstName + " " + c.LastName })
                .FirstOrDefaultAsync(ct);
            if (child is null) return Result.Failure("Child not found.");

            var report = await db.DailyReports.FirstOrDefaultAsync(r => r.ChildId == model.ChildId && r.Date == date, ct);
            if (report is null)
            {
                report = new DailyReport { ChildId = model.ChildId, Date = date };
                db.DailyReports.Add(report);
            }

            report.Mood = model.Mood;
            report.Meals = model.Meals.Trim();
            report.NapMinutes = model.NapMinutes;
            report.Activities = model.Activities.Trim();
            report.TeacherComment = string.IsNullOrWhiteSpace(model.TeacherComment) ? null : model.TeacherComment.Trim();
            report.AuthorName = authorName;

            try
            {
                await db.SaveChangesAsync(ct);
                await notifier.PublishAsync(new ReportSavedEvent(model.ChildId, child.GroupId, child.Name, date));
                return Result.Success();
            }
            catch (DbUpdateException ex) when (attempt == 1 && ex.IsUniqueConstraintViolation())
            {
                logger.LogWarning("Report for child {ChildId} on {Date} was saved by someone else at the same time. Retrying.", model.ChildId, date);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Failed to save daily report for child {ChildId}", model.ChildId);
                return Result.Failure("Could not save the report. Please try again.");
            }
        }

        return Result.Failure("Could not save the report. Please try again.");
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var report = await db.DailyReports.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (report is null) return Result.Failure("Report not found.");

        db.DailyReports.Remove(report);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
