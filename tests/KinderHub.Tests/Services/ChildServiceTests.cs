using KinderHub.Application.DTOs;
using KinderHub.Tests.Helpers;
using Xunit;

namespace KinderHub.Tests.Services;

public class ChildServiceTests
{
    [Fact]
    public async Task Create_valid_child_saves_it()
    {
        using var db = new TestDatabase();
        var groupId = await db.GroupIdAsync("Rainbows");

        var result = await db.Children().CreateAsync(TestData.ValidChild(groupId));

        Assert.True(result.Succeeded, result.Error);
        var found = await db.Children().GetByIdAsync(result.Value);
        Assert.NotNull(found);
        Assert.Equal("Rainbows", found!.GroupName);
        Assert.Equal("test.parent@example.kz", found.ParentEmail); // email is saved in lower case
    }

    [Fact]
    public async Task Create_invalid_child_is_rejected_on_the_server()
    {
        using var db = new TestDatabase();
        var child = TestData.ValidChild(await db.GroupIdAsync("Rainbows"));
        child.ParentEmail = "wrong";

        var result = await db.Children().CreateAsync(child);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Cannot_add_child_to_a_full_group()
    {
        using var db = new TestDatabase();
        var groupResult = await db.Groups().CreateAsync(TestData.ValidGroup("Tiny", capacity: 1));
        var groupId = groupResult.Value;

        var first = await db.Children().CreateAsync(TestData.ValidChild(groupId, "One"));
        var second = await db.Children().CreateAsync(TestData.ValidChild(groupId, "Two"));

        Assert.True(first.Succeeded);
        Assert.False(second.Succeeded);
        Assert.Contains("full", second.Error);
    }

    [Fact]
    public async Task Search_finds_children_by_name_ignoring_case()
    {
        using var db = new TestDatabase();

        var page = await db.Children().GetPagedAsync(new ChildQuery { Search = "SERIK" });

        Assert.Equal(2, page.TotalCount); // Aruzhan Serikova + Alikhan Serikov
    }

    [Fact]
    public async Task Paging_returns_the_right_amount()
    {
        using var db = new TestDatabase();

        var page1 = await db.Children().GetPagedAsync(new ChildQuery { PageSize = 5, Page = 1 });
        var page3 = await db.Children().GetPagedAsync(new ChildQuery { PageSize = 5, Page = 3 });

        Assert.Equal(12, page1.TotalCount);
        Assert.Equal(5, page1.Items.Count);
        Assert.Equal(2, page3.Items.Count);
        Assert.Equal(3, page1.TotalPages);
    }

    [Fact]
    public async Task Parent_sees_only_own_children()
    {
        using var db = new TestDatabase();

        var mine = await db.Children().GetByParentEmailAsync("PARENT@kinderhub.kz");

        Assert.Equal(2, mine.Count);
        Assert.All(mine, c => Assert.Equal("parent@kinderhub.kz", c.ParentEmail));
    }

    [Fact]
    public async Task Deleting_a_child_also_deletes_their_attendance()
    {
        using var db = new TestDatabase();
        var childId = await db.ChildIdAsync("Aruzhan");
        var before = await db.Attendance().GetChildHistoryAsync(childId, TestData.Today.AddDays(-30), TestData.Today);
        Assert.NotEmpty(before);

        var result = await db.Children().DeleteAsync(childId);

        Assert.True(result.Succeeded);
        var after = await db.Attendance().GetChildHistoryAsync(childId, TestData.Today.AddDays(-30), TestData.Today);
        Assert.Empty(after);
    }
}
