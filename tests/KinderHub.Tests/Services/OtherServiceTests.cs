using KinderHub.Application.DTOs;
using KinderHub.Domain.Enums;
using KinderHub.Tests.Helpers;
using Xunit;

namespace KinderHub.Tests.Services;

public class AnnouncementServiceTests
{
    [Fact]
    public async Task Parent_sees_general_news_and_own_group_only()
    {
        using var db = new TestDatabase();
        var sunflowers = await db.GroupIdAsync("Sunflowers");
        var rainbows = await db.GroupIdAsync("Rainbows");

        var forSunflowersParent = await db.Announcements().GetLatestAsync(new[] { sunflowers });

        Assert.NotEmpty(forSunflowersParent);
        Assert.DoesNotContain(forSunflowersParent, a => a.GroupId == rainbows);
    }

    [Fact]
    public async Task Publishing_sends_a_live_new_announcement_event()
    {
        using var db = new TestDatabase();

        var result = await db.Announcements().CreateAsync(
            new AnnouncementEditModel { Title = "Picnic day", Message = "Bring a hat, please." }, "Admin");

        Assert.True(result.Succeeded, result.Error);
        var e = Assert.Single(db.Notifier.AnnouncementEvents);
        Assert.True(e.IsNew);
        Assert.Equal("Picnic day", e.Title);
    }

    [Fact]
    public async Task Announcement_without_title_is_rejected()
    {
        using var db = new TestDatabase();

        var result = await db.Announcements().CreateAsync(new AnnouncementEditModel { Title = "", Message = "Text" }, "Admin");

        Assert.False(result.Succeeded);
        Assert.Empty(db.Notifier.AnnouncementEvents);
    }
}

public class DailyReportServiceTests
{
    [Fact]
    public async Task Report_is_saved_and_parent_gets_a_live_event()
    {
        using var db = new TestDatabase();
        var childId = await db.ChildIdAsync("Aruzhan");

        var result = await db.Reports().SaveAsync(new DailyReportEditModel
        {
            ChildId = childId,
            Date = TestData.Today,
            Mood = Mood.Happy,
            Meals = "Ate everything",
            NapMinutes = 90,
            Activities = "Painting"
        }, "Dana Akhmetova");

        Assert.True(result.Succeeded, result.Error);
        var reports = await db.Reports().GetForChildAsync(childId);
        var report = Assert.Single(reports);
        Assert.Equal("Dana Akhmetova", report.AuthorName);
        Assert.Single(db.Notifier.ReportEvents);
    }
}

public class DashboardServiceTests
{
    [Fact]
    public async Task Stats_match_the_demo_data()
    {
        using var db = new TestDatabase();

        var stats = await db.Dashboard().GetStatsAsync();

        Assert.Equal(12, stats.TotalChildren);
        Assert.Equal(3, stats.TotalGroups);
        Assert.Equal(3, stats.Occupancy.Count);
        Assert.InRange(stats.AttendanceRate14Days, 0, 100);
        Assert.NotEmpty(stats.AttendanceTrend);
    }
}
