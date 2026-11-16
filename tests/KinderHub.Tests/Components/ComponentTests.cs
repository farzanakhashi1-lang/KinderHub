using Bunit;
using KinderHub.Domain.Enums;
using KinderHub.Web.Components.Shared;
using Xunit;

namespace KinderHub.Tests.Components;

// bUnit renders Blazor components in memory and lets us click buttons and read the HTML

public class PagerTests : BunitContext
{
    [Fact]
    public void Is_hidden_when_there_is_only_one_page()
    {
        var cut = Render<Pager>(p => p
            .Add(x => x.Page, 1)
            .Add(x => x.TotalPages, 1)
            .Add(x => x.TotalCount, 5));

        Assert.Empty(cut.FindAll("nav"));
    }

    [Fact]
    public void Next_button_asks_for_the_next_page()
    {
        var requestedPage = 0;
        var cut = Render<Pager>(p => p
            .Add(x => x.Page, 1)
            .Add(x => x.TotalPages, 3)
            .Add(x => x.TotalCount, 25)
            .Add(x => x.PageChanged, (int page) => requestedPage = page));

        cut.Find("button[aria-label='Next page']").Click();

        Assert.Equal(2, requestedPage);
        Assert.Contains("Page 1 of 3", cut.Markup);
    }

    [Fact]
    public void Previous_button_is_disabled_on_the_first_page()
    {
        var cut = Render<Pager>(p => p
            .Add(x => x.Page, 1)
            .Add(x => x.TotalPages, 3)
            .Add(x => x.TotalCount, 25));

        Assert.True(cut.Find("button[aria-label='Previous page']").HasAttribute("disabled"));
        Assert.False(cut.Find("button[aria-label='Next page']").HasAttribute("disabled"));
    }
}

public class ConfirmDialogTests : BunitContext
{
    public ConfirmDialogTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose; // the modal moves keyboard focus with JS
    }

    [Fact]
    public void Shows_nothing_when_closed()
    {
        var cut = Render<ConfirmDialog>(p => p.Add(x => x.IsOpen, false).Add(x => x.Message, "Hidden"));

        Assert.Empty(cut.FindAll(".kh-modal"));
    }

    [Fact]
    public void Shows_the_message_and_calls_OnConfirm()
    {
        var confirmed = false;
        var cut = Render<ConfirmDialog>(p => p
            .Add(x => x.IsOpen, true)
            .Add(x => x.Title, "Delete child?")
            .Add(x => x.Message, "Aruzhan will be deleted.")
            .Add(x => x.ConfirmText, "Delete")
            .Add(x => x.OnConfirm, () => confirmed = true));

        Assert.Contains("Aruzhan will be deleted.", cut.Markup);
        Assert.NotNull(cut.Find("[role='dialog'][aria-modal='true']")); // accessible dialog

        cut.FindAll("button").Single(b => b.TextContent.Trim() == "Delete").Click();

        Assert.True(confirmed);
    }

    [Fact]
    public void Cancel_calls_OnCancel()
    {
        var cancelled = false;
        var cut = Render<ConfirmDialog>(p => p
            .Add(x => x.IsOpen, true)
            .Add(x => x.Message, "Sure?")
            .Add(x => x.OnCancel, () => cancelled = true));

        cut.FindAll("button").Single(b => b.TextContent.Trim() == "Cancel").Click();

        Assert.True(cancelled);
    }
}

public class DataTableTests : BunitContext
{
    [Fact]
    public void Renders_one_row_per_item()
    {
        var cut = Render<DataTable<string>>(p => p
            .Add(x => x.Items, new[] { "Aruzhan", "Alikhan", "Amina" })
            .Add(x => x.Header, "<th>Name</th>")
            .Add(x => x.Row, name => $"<td>{name}</td>"));

        Assert.Equal(3, cut.FindAll("tbody tr").Count);
        Assert.Contains("Alikhan", cut.Markup);
    }

    [Fact]
    public void Shows_empty_state_when_there_are_no_items()
    {
        var cut = Render<DataTable<string>>(p => p
            .Add(x => x.Items, Array.Empty<string>())
            .Add(x => x.Header, "<th>Name</th>")
            .Add(x => x.Row, name => $"<td>{name}</td>")
            .Add(x => x.EmptyTitle, "No children found"));

        Assert.Empty(cut.FindAll("table"));
        Assert.Equal("No children found", cut.Find(".kh-empty-title").TextContent);
    }

    [Fact]
    public void Shows_loading_text_while_loading()
    {
        var cut = Render<DataTable<string>>(p => p
            .Add(x => x.Items, (IReadOnlyList<string>?)null)
            .Add(x => x.Header, "<th>Name</th>")
            .Add(x => x.Row, name => $"<td>{name}</td>")
            .Add(x => x.IsLoading, true));

        Assert.NotNull(cut.Find("[role='status']"));
    }
}

public class SmallComponentTests : BunitContext
{
    [Theory]
    [InlineData(AttendanceStatus.Present, "Present", "kh-badge-success")]
    [InlineData(AttendanceStatus.Absent, "Absent", "kh-badge-danger")]
    public void AttendanceBadge_shows_text_and_color(AttendanceStatus status, string text, string cssClass)
    {
        var cut = Render<AttendanceBadge>(p => p.Add(x => x.Status, status));

        var badge = cut.Find(".kh-badge");
        Assert.Equal(text, badge.TextContent);
        Assert.Contains(cssClass, badge.ClassList);
    }

    [Fact]
    public void AttendanceBadge_without_status_says_not_marked()
    {
        var cut = Render<AttendanceBadge>(p => p.Add(x => x.Status, (AttendanceStatus?)null));
        Assert.Equal("Not marked", cut.Find(".kh-badge").TextContent);
    }

    [Fact]
    public void StatCard_shows_value_title_and_hint()
    {
        var cut = Render<StatCard>(p => p
            .Add(x => x.Title, "Children")
            .Add(x => x.Value, "12")
            .Add(x => x.Hint, "3 groups")
            .Add(x => x.Color, "success"));

        Assert.Equal("12", cut.Find(".kh-stat-value").TextContent);
        Assert.Equal("Children", cut.Find(".kh-stat-title").TextContent);
        Assert.Contains("kh-stat-success", cut.Find(".kh-stat").ClassList);
    }

    [Fact]
    public void Icon_falls_back_to_info_for_unknown_names()
    {
        var cut = Render<Icon>(p => p.Add(x => x.Name, "does-not-exist"));
        Assert.NotNull(cut.Find("svg[aria-hidden='true']"));
    }
}
