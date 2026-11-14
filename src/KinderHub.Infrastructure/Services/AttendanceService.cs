using KinderHub.Application.Common;
using KinderHub.Application.DTOs;
using KinderHub.Application.Interfaces;
using KinderHub.Domain.Entities;
using KinderHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KinderHub.Infrastructure.Services;

public class AttendanceService(
    IDbContextFactory<KinderHubDbContext> dbFactory,
    IRealtimeNotifier notifier,
    ILogger<AttendanceService> logger) : IAttendanceService
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
        var validation = ModelValidator.Validate(model);
        if (!validation.Succeeded) return validation;

        var date = model.Date!.Value;

        // Two teachers can save the same child at the same second. The unique index
        // (one record per child per day) protects the data. If we lose that race,
        // we simply try once more: the second time it becomes an UPDATE.
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct);
            var child = await db.Children.AsNoTracking()
                .Where(c => c.Id == model.ChildId)
                .Select(c => new { c.GroupId, Name = c.FirstName + " " + c.LastName })
                .FirstOrDefaultAsync(ct);
            if (child is null) return Result.Failure("Child not found.");

            var record = await db.AttendanceRecords.FirstOrDefaultAsync(a => a.ChildId == model.ChildId && a.Date == date, ct);
            if (record is null)
            {
                record = new AttendanceRecord { ChildId = model.ChildId, Date = date };
                db.AttendanceRecords.Add(record);
            }

            record.Status = model.Status!.Value;
            record.CheckInTime = model.CheckInTime;
            record.CheckOutTime = model.CheckOutTime;
            record.PickedUpBy = string.IsNullOrWhiteSpace(model.PickedUpBy) ? null : model.PickedUpBy.Trim();
            record.Note = string.IsNullOrWhiteSpace(model.Note) ? null : model.Note.Trim();

            try
            {
                await db.SaveChangesAsync(ct);

                // Tell everyone who is online (parents, other teachers, dashboards)
                await notifier.PublishAsync(new AttendanceChangedEvent(model.ChildId, child.GroupId, child.Name, date,
                    record.Status, record.CheckInTime, record.CheckOutTime));
                return Result.Success();
            }
            catch (DbUpdateException ex) when (attempt == 1 && ex.IsUniqueConstraintViolation())
            {
                logger.LogWarning("Attendance for child {ChildId} on {Date} was saved by someone else at the same time. Retrying.", model.ChildId, date);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Failed to save attendance for child {ChildId} on {Date}", model.ChildId, date);
                return Result.Failure("Could not save attendance. Please refresh and try again.");
            }
        }

        return Result.Failure("Could not save attendance. Please refresh and try again.");
    }

    public async Task<PagedResult<AttendanceDto>> GetHistoryAsync(int? groupId, int skip, int take, CancellationToken ct = default)
    {
        skip = Math.Max(0, skip);
        take = Math.Clamp(take, 1, 10_000);

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var query = db.AttendanceRecords.AsNoTracking().AsQueryable();
        if (groupId is > 0)
            query = query.Where(a => a.Child.GroupId == groupId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(a => a.Date).ThenBy(a => a.Child.FirstName)
            .Skip(skip)
            .Take(take)
            .Select(a => new AttendanceDto(a.Id, a.ChildId, a.Child.FirstName + " " + a.Child.LastName, a.Date,
                a.Status, a.CheckInTime, a.CheckOutTime, a.PickedUpBy, a.Note))
            .ToListAsync(ct);

        return new PagedResult<AttendanceDto>(items, total, skip / take + 1, take);
    }
}
