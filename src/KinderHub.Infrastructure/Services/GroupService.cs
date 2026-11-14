using KinderHub.Application.Common;
using KinderHub.Application.DTOs;
using KinderHub.Application.Interfaces;
using KinderHub.Domain.Entities;
using KinderHub.Infrastructure.Caching;
using KinderHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace KinderHub.Infrastructure.Services;

public class GroupService(IDbContextFactory<KinderHubDbContext> dbFactory, IMemoryCache cache, ILogger<GroupService> logger) : IGroupService
{
    // Groups are read on almost every page but change rarely, so we keep them in memory for 5 minutes.
    // Any change to groups or children removes the cached copy (see Invalidate).
    public async Task<IReadOnlyList<GroupDto>> GetAllAsync(CancellationToken ct = default)
    {
        if (cache.TryGetValue(CacheKeys.AllGroups, out IReadOnlyList<GroupDto>? cached) && cached is not null)
            return cached;

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        IReadOnlyList<GroupDto> groups = await db.Groups.AsNoTracking()
            .OrderBy(g => g.Name)
            .Select(g => new GroupDto(g.Id, g.Name, g.AgeRange, g.Room, g.TeacherName, g.Capacity, g.Children.Count))
            .ToListAsync(ct);

        cache.Set(CacheKeys.AllGroups, groups, TimeSpan.FromMinutes(5));
        return groups;
    }

    private void Invalidate() => cache.Remove(CacheKeys.AllGroups);

    public async Task<GroupDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.Groups.AsNoTracking()
            .Where(g => g.Id == id)
            .Select(g => new GroupDto(g.Id, g.Name, g.AgeRange, g.Room, g.TeacherName, g.Capacity, g.Children.Count))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Result<int>> CreateAsync(GroupEditModel model, CancellationToken ct = default)
    {
        var validation = ModelValidator.Validate(model);
        if (!validation.Succeeded) return Result<int>.Failure(validation.Error!);

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var name = model.Name.Trim();
        if (await db.Groups.AnyAsync(g => g.Name.ToLower() == name.ToLower(), ct))
            return Result<int>.Failure($"A group named \"{name}\" already exists.");

        var group = new Group();
        Apply(model, group);
        db.Groups.Add(group);

        try
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Group {GroupId} created", group.Id);
            Invalidate();
            return Result<int>.Success(group.Id);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to create group");
            return Result<int>.Failure("Could not save the group. Please try again.");
        }
    }

    public async Task<Result> UpdateAsync(GroupEditModel model, CancellationToken ct = default)
    {
        var validation = ModelValidator.Validate(model);
        if (!validation.Succeeded) return validation;

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var group = await db.Groups.Include(g => g.Children).FirstOrDefaultAsync(g => g.Id == model.Id, ct);
        if (group is null) return Result.Failure("Group not found.");

        var name = model.Name.Trim();
        if (await db.Groups.AnyAsync(g => g.Id != model.Id && g.Name.ToLower() == name.ToLower(), ct))
            return Result.Failure($"A group named \"{name}\" already exists.");

        if (model.Capacity < group.Children.Count)
            return Result.Failure($"Capacity cannot be less than the current number of children ({group.Children.Count}).");

        Apply(model, group);

        try
        {
            await db.SaveChangesAsync(ct);
            Invalidate();
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to update group {GroupId}", model.Id);
            return Result.Failure("Could not save the group. Please try again.");
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var group = await db.Groups.Include(g => g.Children).FirstOrDefaultAsync(g => g.Id == id, ct);
        if (group is null) return Result.Failure("Group not found.");
        if (group.Children.Count > 0)
            return Result.Failure("This group still has children. Move them to another group first.");

        db.Groups.Remove(group);
        try
        {
            await db.SaveChangesAsync(ct);
            Invalidate();
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to delete group {GroupId}", id);
            return Result.Failure("Could not delete the group. Please try again.");
        }
    }

    private static void Apply(GroupEditModel model, Group group)
    {
        group.Name = model.Name.Trim();
        group.AgeRange = model.AgeRange.Trim();
        group.Room = model.Room.Trim();
        group.TeacherName = string.IsNullOrWhiteSpace(model.TeacherName) ? null : model.TeacherName.Trim();
        group.Capacity = model.Capacity;
    }
}
