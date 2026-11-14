using KinderHub.Application.Common;
using KinderHub.Application.DTOs;
using KinderHub.Application.Interfaces;
using KinderHub.Domain.Entities;
using KinderHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KinderHub.Infrastructure.Services;

public class AnnouncementService(
    IDbContextFactory<KinderHubDbContext> dbFactory,
    IRealtimeNotifier notifier,
    ILogger<AnnouncementService> logger) : IAnnouncementService
{
    public async Task<IReadOnlyList<AnnouncementDto>> GetLatestAsync(IReadOnlyCollection<int>? groupIds = null, int take = 20, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var query = db.Announcements.AsNoTracking().AsQueryable();

        if (groupIds is not null)
        {
            var ids = groupIds.ToList();
            query = query.Where(a => a.GroupId == null || ids.Contains(a.GroupId.Value));
        }

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .Take(Math.Clamp(take, 1, 100))
            .Select(a => new AnnouncementDto(a.Id, a.Title, a.Message, a.IsImportant, a.AuthorName,
                a.GroupId, a.Group != null ? a.Group.Name : null, a.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<Result<AnnouncementDto>> CreateAsync(AnnouncementEditModel model, string authorName, CancellationToken ct = default)
    {
        var validation = ModelValidator.Validate(model);
        if (!validation.Succeeded) return Result<AnnouncementDto>.Failure(validation.Error!);

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        string? groupName = null;
        if (model.GroupId is int gid)
        {
            groupName = await db.Groups.Where(g => g.Id == gid).Select(g => g.Name).FirstOrDefaultAsync(ct);
            if (groupName is null) return Result<AnnouncementDto>.Failure("The selected group does not exist.");
        }

        var announcement = new Announcement
        {
            Title = model.Title.Trim(),
            Message = model.Message.Trim(),
            IsImportant = model.IsImportant,
            GroupId = model.GroupId,
            AuthorName = authorName
        };
        db.Announcements.Add(announcement);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to create announcement");
            return Result<AnnouncementDto>.Failure("Could not publish the announcement. Please try again.");
        }

        await notifier.PublishAsync(new AnnouncementEvent(announcement.Id, announcement.Title, announcement.GroupId,
            announcement.IsImportant, IsNew: true));

        return Result<AnnouncementDto>.Success(new AnnouncementDto(announcement.Id, announcement.Title, announcement.Message,
            announcement.IsImportant, announcement.AuthorName, announcement.GroupId, groupName, announcement.CreatedAt));
    }

    public async Task<Result> UpdateAsync(AnnouncementEditModel model, CancellationToken ct = default)
    {
        var validation = ModelValidator.Validate(model);
        if (!validation.Succeeded) return validation;

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var announcement = await db.Announcements.FirstOrDefaultAsync(a => a.Id == model.Id, ct);
        if (announcement is null) return Result.Failure("Announcement not found.");

        announcement.Title = model.Title.Trim();
        announcement.Message = model.Message.Trim();
        announcement.IsImportant = model.IsImportant;
        announcement.GroupId = model.GroupId;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to update announcement {Id}", model.Id);
            return Result.Failure("Could not save the announcement. Please try again.");
        }

        // Changed (not new): pages refresh, but nobody gets a "new announcement" pop-up
        await notifier.PublishAsync(new AnnouncementEvent(announcement.Id, announcement.Title, announcement.GroupId,
            announcement.IsImportant, IsNew: false));
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var announcement = await db.Announcements.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (announcement is null) return Result.Failure("Announcement not found.");

        db.Announcements.Remove(announcement);
        await db.SaveChangesAsync(ct);

        await notifier.PublishAsync(new AnnouncementEvent(id, announcement.Title, announcement.GroupId,
            announcement.IsImportant, IsNew: false));
        return Result.Success();
    }
}
