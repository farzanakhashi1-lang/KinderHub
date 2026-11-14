using KinderHub.Web.Services;
using Xunit;

namespace KinderHub.Tests.Unit;

public class ToastServiceTests
{
    [Fact]
    public void Show_adds_a_toast_and_raises_OnChange()
    {
        var service = new ToastService();
        var changes = 0;
        service.OnChange += () => changes++;

        service.ShowSuccess("Saved!");

        var toast = Assert.Single(service.GetToasts());
        Assert.Equal("Saved!", toast.Text);
        Assert.Equal(ToastLevel.Success, toast.Level);
        Assert.Equal(1, changes);
    }

    [Fact]
    public void Remove_deletes_the_toast()
    {
        var service = new ToastService();
        service.ShowError("Something went wrong");
        var id = service.GetToasts()[0].Id;

        service.Remove(id);

        Assert.Empty(service.GetToasts());
    }

    [Fact]
    public async Task Toast_disappears_by_itself()
    {
        var service = new ToastService();
        service.Show("Short message", ToastLevel.Info, durationMs: 50);

        await Task.Delay(300);

        Assert.Empty(service.GetToasts());
    }
}
