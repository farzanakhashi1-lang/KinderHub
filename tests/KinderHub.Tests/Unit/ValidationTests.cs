using KinderHub.Application.Common;
using KinderHub.Application.DTOs;
using KinderHub.Domain.Enums;
using KinderHub.Tests.Helpers;
using Xunit;

namespace KinderHub.Tests.Unit;

// Business rules on the forms (the same rules run on the server via ModelValidator)
public class ValidationTests
{
    [Fact]
    public void Valid_child_passes()
    {
        var result = ModelValidator.Validate(TestData.ValidChild(groupId: 1));
        Assert.True(result.Succeeded, result.Error);
    }

    [Fact]
    public void Child_without_first_name_fails()
    {
        var child = TestData.ValidChild(1);
        child.FirstName = "";

        var result = ModelValidator.Validate(child);

        Assert.False(result.Succeeded);
        Assert.Contains("First name is required", result.Error);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("abc@")]
    public void Child_with_invalid_parent_email_fails(string email)
    {
        var child = TestData.ValidChild(1);
        child.ParentEmail = email;

        Assert.False(ModelValidator.Validate(child).Succeeded);
    }

    [Theory]
    [InlineData(1)]   // born tomorrow (future)
    [InlineData(-30)] // 30 days old: younger than 1 year
    public void Child_too_young_or_not_born_fails(int daysFromToday)
    {
        var child = TestData.ValidChild(1);
        child.DateOfBirth = TestData.Today.AddDays(daysFromToday);

        Assert.False(ModelValidator.Validate(child).Succeeded);
    }

    [Fact]
    public void Child_older_than_8_fails()
    {
        var child = TestData.ValidChild(1);
        child.DateOfBirth = TestData.Today.AddYears(-9);

        var result = ModelValidator.Validate(child);

        Assert.False(result.Succeeded);
        Assert.Contains("younger than 8", result.Error);
    }

    [Fact]
    public void Child_without_group_fails()
    {
        var child = TestData.ValidChild(0);
        Assert.False(ModelValidator.Validate(child).Succeeded);
    }

    [Fact]
    public void Group_capacity_must_be_between_1_and_40()
    {
        Assert.False(ModelValidator.Validate(TestData.ValidGroup(capacity: 0)).Succeeded);
        Assert.False(ModelValidator.Validate(TestData.ValidGroup(capacity: 41)).Succeeded);
        Assert.True(ModelValidator.Validate(TestData.ValidGroup(capacity: 40)).Succeeded);
    }

    [Fact]
    public void Attendance_check_out_before_check_in_fails()
    {
        var model = new AttendanceEditModel
        {
            ChildId = 1,
            Date = TestData.Today,
            Status = AttendanceStatus.Present,
            CheckInTime = new TimeOnly(9, 0),
            CheckOutTime = new TimeOnly(8, 0)
        };

        var result = ModelValidator.Validate(model);

        Assert.False(result.Succeeded);
        Assert.Contains("Check-out must be after check-in", result.Error);
    }

    [Fact]
    public void Absent_child_cannot_have_check_in_time()
    {
        var model = new AttendanceEditModel
        {
            ChildId = 1,
            Date = TestData.Today,
            Status = AttendanceStatus.Absent,
            CheckInTime = new TimeOnly(8, 30)
        };

        Assert.False(ModelValidator.Validate(model).Succeeded);
    }

    [Fact]
    public void Attendance_for_future_date_fails()
    {
        var model = new AttendanceEditModel
        {
            ChildId = 1,
            Date = TestData.Today.AddDays(1),
            Status = AttendanceStatus.Present,
            CheckInTime = new TimeOnly(8, 30)
        };

        Assert.False(ModelValidator.Validate(model).Succeeded);
    }

    [Fact]
    public void Daily_report_nap_over_240_minutes_fails()
    {
        var model = new DailyReportEditModel
        {
            ChildId = 1,
            Meals = "All",
            Activities = "Drawing",
            NapMinutes = 300
        };

        Assert.False(ModelValidator.Validate(model).Succeeded);
    }
}
