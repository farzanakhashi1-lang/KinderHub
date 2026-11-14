using KinderHub.Tests.Helpers;
using Xunit;

namespace KinderHub.Tests.Services;

public class GroupServiceTests
{
    [Fact]
    public async Task Group_names_must_be_unique()
    {
        using var db = new TestDatabase();

        var result = await db.Groups().CreateAsync(TestData.ValidGroup("sunflowers")); // already exists (different case)

        Assert.False(result.Succeeded);
        Assert.Contains("already exists", result.Error);
    }

    [Fact]
    public async Task Group_with_children_cannot_be_deleted()
    {
        using var db = new TestDatabase();
        var groupId = await db.GroupIdAsync("Sunflowers");

        var result = await db.Groups().DeleteAsync(groupId);

        Assert.False(result.Succeeded);
        Assert.Contains("still has children", result.Error);
    }

    [Fact]
    public async Task Empty_group_can_be_deleted()
    {
        using var db = new TestDatabase();
        var created = await db.Groups().CreateAsync(TestData.ValidGroup("Temporary"));

        var result = await db.Groups().DeleteAsync(created.Value);

        Assert.True(result.Succeeded);
        Assert.DoesNotContain(await db.Groups().GetAllAsync(), g => g.Name == "Temporary");
    }

    [Fact]
    public async Task Capacity_cannot_go_below_current_children()
    {
        using var db = new TestDatabase();
        var group = (await db.Groups().GetAllAsync()).Single(g => g.Name == "Sunflowers");
        var model = KinderHub.Application.DTOs.GroupEditModel.FromDto(group);
        model.Capacity = 1;

        var result = await db.Groups().UpdateAsync(model);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Cached_group_list_is_refreshed_when_a_child_is_added()
    {
        using var db = new TestDatabase();
        var before = (await db.Groups().GetAllAsync()).Single(g => g.Name == "Rainbows").ChildrenCount; // now cached

        await db.Children().CreateAsync(TestData.ValidChild(await db.GroupIdAsync("Rainbows")));

        var after = (await db.Groups().GetAllAsync()).Single(g => g.Name == "Rainbows").ChildrenCount;
        Assert.Equal(before + 1, after);
    }
}
