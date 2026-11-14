using KinderHub.Application.DTOs;

namespace KinderHub.Tests.Helpers;

// Ready-made valid models. Each test changes only what it wants to check.
public static class TestData
{
    public static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    public static ChildEditModel ValidChild(int groupId, string lastName = "Testov") => new()
    {
        FirstName = "Timur",
        LastName = lastName,
        DateOfBirth = Today.AddYears(-4),
        GroupId = groupId,
        ParentName = "Test Parent",
        ParentEmail = "Test.Parent@Example.kz",
        ParentPhone = "+7 700 000 0000"
    };

    public static GroupEditModel ValidGroup(string name = "Tiny Tots", int capacity = 10) => new()
    {
        Name = name,
        AgeRange = "2-3 years",
        Room = "001",
        TeacherName = "Test Teacher",
        Capacity = capacity
    };
}
