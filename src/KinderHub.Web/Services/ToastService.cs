namespace KinderHub.Web.Services;

public enum ToastLevel { Success, Error, Info, Warning }

public record ToastMessage(Guid Id, string Text, ToastLevel Level);

// Small pop-up messages ("Saved!", "Something went wrong") shown in the corner.
public class ToastService
{
    private readonly List<ToastMessage> _toasts = new();
    private readonly object _lock = new();

    public event Action? OnChange;

    public IReadOnlyList<ToastMessage> GetToasts()
    {
        lock (_lock) return _toasts.ToList();
    }

    public void ShowSuccess(string text) => Show(text, ToastLevel.Success);
    public void ShowError(string text) => Show(text, ToastLevel.Error, 6000);
    public void ShowInfo(string text) => Show(text, ToastLevel.Info);
    public void ShowWarning(string text) => Show(text, ToastLevel.Warning);

    public void Show(string text, ToastLevel level, int durationMs = 4000)
    {
        var toast = new ToastMessage(Guid.NewGuid(), text, level);
        lock (_lock) _toasts.Add(toast);
        OnChange?.Invoke();
        _ = RemoveLaterAsync(toast.Id, durationMs);
    }

    public void Remove(Guid id)
    {
        lock (_lock) _toasts.RemoveAll(t => t.Id == id);
        OnChange?.Invoke();
    }

    private async Task RemoveLaterAsync(Guid id, int delayMs)
    {
        await Task.Delay(delayMs);
        Remove(id);
    }
}
