using System.Linq.Expressions;
using KinderHub.Application.Common;
using KinderHub.Application.DTOs;
using KinderHub.Application.Interfaces;
using KinderHub.Domain.Entities;
using KinderHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KinderHub.Infrastructure.Services;

public class ChildService(IDbContextFactory<KinderHubDbContext> dbFactory, ILogger<ChildService> logger) : IChildService
{
    // Converts a Child (database) into a ChildDto (page) inside the SQL query,
    // so only the needed columns are loaded.
    private static readonly Expression<Func<Child, ChildDto>> ToDto = c => new ChildDto
    {
        Id = c.Id,
        FirstName = c.FirstName,
        LastName = c.LastName,
        DateOfBirth = c.DateOfBirth,
        GroupId = c.GroupId,
        GroupName = c.Group.Name,
        ParentName = c.ParentName,
        ParentEmail = c.ParentEmail,
        ParentPhone = c.ParentPhone,
        Notes = c.Notes
    };

    public async Task<PagedResult<ChildDto>> GetPagedAsync(ChildQuery query, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var children = db.Children.AsNoTracking().AsQueryable();

        if (query.GroupId is > 0)
            children = children.Where(c => c.GroupId == query.GroupId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLower();
            children = children.Where(c =>
                c.FirstName.ToLower().Contains(s) ||
                c.LastName.ToLower().Contains(s) ||
                c.ParentName.ToLower().Contains(s));
        }

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var total = await children.CountAsync(ct);

        var items = await children
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToDto)
            .ToListAsync(ct);

        return new PagedResult<ChildDto>(items, total, page, pageSize);
    }

    public async Task<IReadOnlyList<ChildDto>> GetByGroupAsync(int groupId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.Children.AsNoTracking()
            .Where(c => c.GroupId == groupId)
            .OrderBy(c => c.FirstName)
            .Select(ToDto)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ChildDto>> GetByParentEmailAsync(string parentEmail, CancellationToken ct = default)
    {
        var email = parentEmail.Trim().ToLower();
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.Children.AsNoTracking()
            .Where(c => c.ParentEmail.ToLower() == email)
            .OrderBy(c => c.FirstName)
            .Select(ToDto)
            .ToListAsync(ct);
    }

    public async Task<ChildDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.Children.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Result<int>> CreateAsync(ChildEditModel model, CancellationToken ct = default)
    {
        var validation = ModelValidator.Validate(model);
        if (!validation.Succeeded) return Result<int>.Failure(validation.Error!);

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var groupCheck = await CheckGroupAsync(db, model.GroupId, ct);
        if (groupCheck is not null) return Result<int>.Failure(groupCheck);

        var child = new Child();
        Apply(model, child);
        db.Children.Add(child);

        try
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Child {ChildId} created", child.Id);
            return Result<int>.Success(child.Id);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to create child");
            return Result<int>.Failure("Could not save the child. Please try again.");
        }
    }

    public async Task<Result> UpdateAsync(ChildEditModel model, CancellationToken ct = default)
    {
        var validation = ModelValidator.Validate(model);
        if (!validation.Succeeded) return validation;

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var child = await db.Children.FirstOrDefaultAsync(c => c.Id == model.Id, ct);
        if (child is null) return Result.Failure("Child not found.");

        if (child.GroupId != model.GroupId)
        {
            var groupCheck = await CheckGroupAsync(db, model.GroupId, ct);
            if (groupCheck is not null) return Result.Failure(groupCheck);
        }

        Apply(model, child);

        try
        {
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to update child {ChildId}", model.Id);
            return Result.Failure("Could not save the child. Please try again.");
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var child = await db.Children.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (child is null) return Result.Failure("Child not found.");

        db.Children.Remove(child);
        try
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Child {ChildId} deleted", id);
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to delete child {ChildId}", id);
            return Result.Failure("Could not delete the child. Please try again.");
        }
    }

    // Returns an error message, or null if the group is OK
    private static async Task<string?> CheckGroupAsync(KinderHubDbContext db, int groupId, CancellationToken ct)
    {
        var group = await db.Groups.AsNoTracking()
            .Where(g => g.Id == groupId)
            .Select(g => new { g.Name, g.Capacity, Count = g.Children.Count })
            .FirstOrDefaultAsync(ct);

        if (group is null) return "The selected group does not exist.";
        if (group.Count >= group.Capacity) return $"Group \"{group.Name}\" is full ({group.Capacity} children).";
        return null;
    }

    private static void Apply(ChildEditModel model, Child child)
    {
        child.FirstName = model.FirstName.Trim();
        child.LastName = model.LastName.Trim();
        child.DateOfBirth = model.DateOfBirth!.Value;
        child.GroupId = model.GroupId;
        child.ParentName = model.ParentName.Trim();
        child.ParentEmail = model.ParentEmail.Trim().ToLowerInvariant();
        child.ParentPhone = model.ParentPhone.Trim();
        child.Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim();
    }
}
