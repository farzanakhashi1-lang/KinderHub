using KinderHub.Domain.Enums;

namespace KinderHub.Application.Interfaces;

// Messages that are sent live to everyone who is online
public record AttendanceChangedEvent(int ChildId, int GroupId, string ChildName, DateOnly Date,
    AttendanceStatus? Status, TimeOnly? CheckInTime, TimeOnly? CheckOutTime);

public record ReportSavedEvent(int ChildId, int GroupId, string ChildName, DateOnly Date);

public record AnnouncementEvent(int Id, string Title, int? GroupId, bool IsImportant, bool IsNew);

// Publish / subscribe: services PUBLISH changes, pages SUBSCRIBE and update themselves
public interface IRealtimeNotifier
{
    event Action<AttendanceChangedEvent>? AttendanceChanged;
    event Action<ReportSavedEvent>? ReportSaved;
    event Action<AnnouncementEvent>? AnnouncementChanged;

    Task PublishAsync(AttendanceChangedEvent e);
    Task PublishAsync(ReportSavedEvent e);
    Task PublishAsync(AnnouncementEvent e);
}
