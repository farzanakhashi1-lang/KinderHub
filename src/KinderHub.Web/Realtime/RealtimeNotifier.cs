using KinderHub.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace KinderHub.Web.Realtime;

// ONE shared object for the whole app (singleton).
// 1) In-app events: every open Blazor page that subscribed is updated instantly.
//    (Blazor Server sends the new UI to each browser over its own SignalR connection.)
// 2) SignalR hub broadcast: new announcements are pushed to every connected hub client.
public class RealtimeNotifier(IHubContext<NotificationHub> hubContext, ILogger<RealtimeNotifier> logger) : IRealtimeNotifier
{
    public event Action<AttendanceChangedEvent>? AttendanceChanged;
    public event Action<ReportSavedEvent>? ReportSaved;
    public event Action<AnnouncementEvent>? AnnouncementChanged;

    public Task PublishAsync(AttendanceChangedEvent e)
    {
        Raise(AttendanceChanged, e);
        return Task.CompletedTask;
    }

    public Task PublishAsync(ReportSavedEvent e)
    {
        Raise(ReportSaved, e);
        return Task.CompletedTask;
    }

    public async Task PublishAsync(AnnouncementEvent e)
    {
        Raise(AnnouncementChanged, e);

        if (e.IsNew)
        {
            try
            {
                await hubContext.Clients.All.SendAsync(HubMethods.AnnouncementPublished, e);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not broadcast announcement {Id}", e.Id);
            }
        }
    }

    // Calls every subscriber separately: if one page fails, the others still get the message
    private void Raise<T>(Action<T>? handler, T e)
    {
        if (handler is null) return;
        foreach (var subscriber in handler.GetInvocationList().Cast<Action<T>>())
        {
            try
            {
                subscriber(e);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "A live update subscriber failed");
            }
        }
    }
}
