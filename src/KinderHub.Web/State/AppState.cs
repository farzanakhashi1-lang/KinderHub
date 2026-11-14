using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace KinderHub.Web.State;

// State container: shared UI state for one user (one browser tab).
// Components subscribe to OnChange and re-render when something changes.
public class AppState(ProtectedLocalStorage storage)
{
    private const string DarkModeKey = "kh-dark-mode";

    public bool IsDarkMode { get; private set; }
    public bool IsLoaded { get; private set; }

    // How many new announcements arrived live since the user last opened the Announcements page
    public int UnreadAnnouncements { get; private set; }

    public event Action? OnChange;

    // Reads the saved theme from the browser (encrypted local storage)
    public async Task LoadAsync()
    {
        if (IsLoaded) return;
        try
        {
            var result = await storage.GetAsync<bool>(DarkModeKey);
            IsDarkMode = result.Success && result.Value;
        }
        catch
        {
            IsDarkMode = false; // storage not available: use light theme
        }
        IsLoaded = true;
        NotifyStateChanged();
    }

    public async Task ToggleThemeAsync()
    {
        IsDarkMode = !IsDarkMode;
        NotifyStateChanged();
        try
        {
            await storage.SetAsync(DarkModeKey, IsDarkMode);
        }
        catch
        {
            // Not critical: the theme still works for this session
        }
    }

    public void IncrementUnreadAnnouncements()
    {
        UnreadAnnouncements++;
        NotifyStateChanged();
    }

    public void ClearUnreadAnnouncements()
    {
        if (UnreadAnnouncements == 0) return;
        UnreadAnnouncements = 0;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
