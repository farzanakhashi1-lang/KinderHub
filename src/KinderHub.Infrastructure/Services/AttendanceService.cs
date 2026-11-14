using KinderHub.Application.Common;
using KinderHub.Application.DTOs;
using KinderHub.Application.Interfaces;
using KinderHub.Domain.Entities;
using KinderHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KinderHub.Infrastructure.Services;

public class AttendanceService(IDbContextFactory<KinderHubDbContext> dbFactory, ILogger<AttendanceService> logger) : IAttendanceService
{
    public async Task<IReadOnlyList<AttendanceRowDto>> GetDailySheetAsync(int groupId, DateOnly date, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var children = await db.Children.AsNoTracking()
            .Where(c => c.GroupId == groupId)
            .OrderBy(c => c.FirstName)
            .Select(c => new { c.Id, Name = c.FirstName + " " + c.LastName })
            .ToListAsync(ct);

        var childIds = children.Select(c => c.Id).ToList();
        var records = await db.AttendanceRecords.AsNoTracking()
            .Where(a => a.Date == date && childIds.Contains(a.ChildId))
            .ToDictionaryAsync(a => a.ChildId, ct);

        return children.Select(c =>
        {
            records.TryGetValue(c.Id, out var r);
            return new AttendanceRowDto
            {
                ChildId = c.Id,
                ChildName = c.Name,
                RecordId = r?.Id,
                Status = r?.Status,
                CheckInTime = r?.CheckInTime,
                CheckOutTime = r?.CheckOutTime,
                PickedUpBy = r?.PickedUpBy,
                Note = r?.Note
            };
        }).ToList();
    }

    public async Task<IReadOnlyList<AttendanceDto>> GetChildHistoryAsync(int childId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.AttendanceRecords.AsNoTracking()
            .Where(a => a.ChildId == childId && a.Date >= from && a.Date <= to)
            .OrderByDescending(a => a.Date)
            .Select(a => new AttendanceDto(a.Id, a.ChildId, a.Child.FirstName + " " + a.Child.LastName, a.Date,
                a.Status, a.CheckInTime, a.CheckOutTime, a.PickedUpBy, a.Note))
            .ToListAsync(ct);
    }

    // Creates the record, or updates it if one already exists for this child and day
    public async Task<Result> SaveAsync(AttendanceEditModel model, CancellationToken ct = default)
    {
        if (model.Date is null || model.Status is null) return Result.Failure("Date and status are required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        if (!await db.Children.AnyAsync(c => c.Id == model.ChildId, ct))
            return Result.Failure("Child not found.");

        var date = model.Date.Value;
        var record = await db.AttendanceRecords.FirstOrDefaultAsync(a => a.ChildId == model.ChildId && a.Date == date, ct);
        if (record is null)
        {
            record = new AttendanceRecord { ChildId = model.ChildId, Date = date };
            db.AttendanceRecords.Add(record);
        }

        record.Status = model.Status.Value;
        record.CheckInTime = model.CheckInTime;
        record.CheckOutTime = model.CheckOutTime;
        record.PickedUpBy = string.IsNullOrWhiteSpace(model.PickedUpBy) ? null : model.PickedUpBy.Trim();
        record.Note = string.IsNullOrWhiteSpace(model.Note) ? null : model.Note.Trim();

        try
        {
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to save attendance for child {ChildId} on {Date}", model.ChildId, date);
            return Result.Failure("Could not save attendance. Please refresh and try again.");
        }
    }
}
