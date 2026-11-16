namespace KinderHub.Application.Common;

// The 3 user roles of KinderHub
public static class Roles
{
    public const string Admin = "Admin";
    public const string Teacher = "Teacher";
    public const string Parent = "Parent";

    public const string AdminOrTeacher = "Admin,Teacher";

    public static readonly string[] All = { Admin, Teacher, Parent };
}

// Extra information we store about a user
public static class KinderHubClaims
{
    public const string FullName = "full_name";
}
