using KinderHub.Application.Common;
using KinderHub.Application.DTOs;
using KinderHub.Domain.Entities;
using KinderHub.Domain.Enums;
using KinderHub.Web.Services;
using Xunit;

namespace KinderHub.Tests.Unit;

public class CommonTests
{
    [Fact]
    public void Result_success_and_failure()
    {
        Assert.True(Result.Success().Succeeded);

        var failure = Result.Failure("Oops");
        Assert.False(failure.Succeeded);
        Assert.Equal("Oops", failure.Error);

        var withValue = Result<int>.Success(42);
        Assert.Equal(42, withValue.Value);
    }

    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(10, 10, 1)]
    [InlineData(11, 10, 2)]
    [InlineData(57, 10, 6)]
    public void PagedResult_calculates_total_pages(int total, int pageSize, int expectedPages)
    {
        var page = new PagedResult<string>(Array.Empty<string>(), total, 1, pageSize);
        Assert.Equal(expectedPages, page.TotalPages);
    }

    [Fact]
    public void PagedResult_previous_and_next()
    {
        var middle = new PagedResult<string>(Array.Empty<string>(), 30, 2, 10);
        Assert.True(middle.HasPrevious);
        Assert.True(middle.HasNext);

        var last = new PagedResult<string>(Array.Empty<string>(), 30, 3, 10);
        Assert.False(last.HasNext);
    }

    [Fact]
    public void Child_age_is_calculated_correctly_around_birthday()
    {
        var child = new Child { DateOfBirth = new DateOnly(2021, 6, 15) };

        Assert.Equal(4, child.GetAge(new DateOnly(2026, 6, 14))); // day before birthday
        Assert.Equal(5, child.GetAge(new DateOnly(2026, 6, 15))); // birthday
    }

    [Theory]
    [InlineData(AttendanceStatus.Present, "success")]
    [InlineData(AttendanceStatus.Late, "warning")]
    [InlineData(AttendanceStatus.Absent, "danger")]
    [InlineData(AttendanceStatus.Sick, "accent")]
    [InlineData(null, "neutral")]
    public void Status_colors(AttendanceStatus? status, string expected)
    {
        Assert.Equal(expected, DisplayHelpers.StatusColor(status));
    }

    [Fact]
    public void Every_mood_has_an_emoji()
    {
        foreach (var mood in Enum.GetValues<Mood>())
        {
            Assert.False(string.IsNullOrWhiteSpace(DisplayHelpers.MoodEmoji(mood)));
        }
    }

    [Fact]
    public void ChildEditModel_FromDto_copies_all_fields()
    {
        var dto = new ChildDto
        {
            Id = 7, FirstName = "Amina", LastName = "Z", DateOfBirth = new DateOnly(2022, 1, 1), GroupId = 2,
            ParentName = "P", ParentEmail = "p@x.kz", ParentPhone = "1", Notes = "n"
        };

        var model = ChildEditModel.FromDto(dto);

        Assert.Equal(7, model.Id);
        Assert.Equal("Amina", model.FirstName);
        Assert.Equal(2, model.GroupId);
        Assert.Equal("p@x.kz", model.ParentEmail);
    }
}
