namespace KinderHub.Application.Interfaces;

// Who is logged in right now?
public interface ICurrentUserService
{
    Task<CurrentUserInfo> GetCurrentUserAsync();
}

public record CurrentUserInfo(bool IsAuthenticated, string Email, string DisplayName, IReadOnlyList<string> RoleNames)
{
    public static CurrentUserInfo Anonymous { get; } = new(false, string.Empty, "Guest", Array.Empty<string>());

    public bool IsInRole(string role) => RoleNames.Contains(role);
    public bool IsAdmin => IsInRole(KinderHub.Application.Common.Roles.Admin);
    public bool IsTeacher => IsInRole(KinderHub.Application.Common.Roles.Teacher);
    public bool IsParent => IsInRole(KinderHub.Application.Common.Roles.Parent);
}
