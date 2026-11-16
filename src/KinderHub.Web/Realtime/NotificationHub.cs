using Microsoft.AspNetCore.SignalR;

namespace KinderHub.Web.Realtime;

// SignalR hub: a live, two-way connection. The server PUSHES messages to connected clients.
// Clients connect to /hubs/notifications.
public class NotificationHub(ILogger<NotificationHub> logger) : Hub
{
    public override Task OnConnectedAsync()
    {
        logger.LogInformation("Live client connected: {ConnectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("Live client disconnected: {ConnectionId}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}

// Names of the messages the hub sends (one place, no typos)
public static class HubMethods
{
    public const string AnnouncementPublished = "AnnouncementPublished";
}
