using KinderHub.Application.DTOs;
using KinderHub.Domain.Enums;
using KinderHub.Tests.Helpers;
using Xunit;

namespace KinderHub.Tests.Services;

public class AttendanceServiceTests
{
    private static AttendanceEditModel Mark(int childId, AttendanceStatus status, TimeOnly? checkIn = null, TimeOnly? checkOut = null) => new()
    {
        ChildId = childId,
        Date = TestData.Today,
        Status = status,
        CheckInTime = checkIn,
        CheckOutTime = checkOut
    };

    [Fact]
    public async Task Saving_twice_on_the_same_day_updates_instead_of_duplicating()
    {
        using var db = new TestDatabase();
        var childId = await db.ChildIdAsync("Amina");
        var service = db.Attendance();

        await service.SaveAsync(Mark(childId, AttendanceStatus.Late, new TimeOnly(9, 10)));
        var second = await service.SaveAsync(Mark(childId, AttendanceStatus.Late, new TimeOnly(9, 10), new TimeOnly(17, 0)));

        Assert.True(second.Succeeded, second.Error);
        var today = await service.GetChildHistoryAsync(childId, TestData.Today, TestData.Today);
        var record = Assert.Single(today);
        Assert.Equal(new TimeOnly(17, 0), record.CheckOutTime);
    }

    [Fact]
    public async Task Saving_publishes_a_live_event()
    {
        using var db = new TestDatabase();
        var childId = await db.ChildIdAsync("Amina");

        await db.Attendance().SaveAsync(Mark(childId, AttendanceStatus.Present, new TimeOnly(8, 30)));

        var e = Assert.Single(db.Notifier.AttendanceEvents);
        Assert.Equal(childId, e.ChildId);
        Assert.Equal("Amina Zhumabekova", e.ChildName);
        Assert.Equal(AttendanceStatus.Present, e.Status);
    }

    [Fact]
    public async Task Invalid_attendance_is_not_saved_and_not_published()
    {
        using var db = new TestDatabase();
        var childId = await db.ChildIdAsync("Amina");

        var result = await db.Attendance().SaveAsync(Mark(childId, AttendanceStatus.Present, new TimeOnly(10, 0), new TimeOnly(9, 0)));

        Assert.False(result.Succeeded);
        Assert.Empty(db.Notifier.AttendanceEvents);
    }

    [Fact]
    public async Task Unknown_child_is_rejected()
    {
        using var db = new TestDatabase();

        var result = await db.Attendance().SaveAsync(Mark(99999, AttendanceStatus.Present, new TimeOnly(8, 0)));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Daily_sheet_lists_every_child_of_the_group()
    {
        using var db = new TestDatabase();
        var groupId = await db.GroupIdAsync("Sunflowers");

        var sheet = await db.Attendance().GetDailySheetAsync(groupId, TestData.Today);

        Assert.Equal(3, sheet.Count);
        Assert.All(sheet, row => Assert.Null(row.Status)); // nothing marked today yet
    }

    [Fact]
    public async Task History_is_loaded_in_pages()
    {
        using var db = new TestDatabase();
        var service = db.Attendance();

        var first = await service.GetHistoryAsync(null, 0, 5);
        var beyond = await service.GetHistoryAsync(null, first.TotalCount, 5);

        Assert.Equal(5, first.Items.Count);
        Assert.True(first.TotalCount > 5);
        Assert.Empty(beyond.Items);
        Assert.True(first.Items[0].Date >= first.Items[^1].Date); // newest first
    }
}
