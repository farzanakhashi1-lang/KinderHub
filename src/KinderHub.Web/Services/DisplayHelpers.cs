using KinderHub.Domain.Enums;

namespace KinderHub.Web.Services;

// Small helpers to show values nicely on the pages
public static class DisplayHelpers
{
    public static string MoodEmoji(Mood mood) => mood switch
    {
        Mood.Happy => "😊",
        Mood.Calm => "🙂",
        Mood.Tired => "😴",
        Mood.Upset => "😢",
        _ => "🙂"
    };

    public static string StatusColor(AttendanceStatus? status) => status switch
    {
        AttendanceStatus.Present => "success",
        AttendanceStatus.Late => "warning",
        AttendanceStatus.Absent => "danger",
        AttendanceStatus.Sick => "accent",
        _ => "neutral"
    };

    public static bool CameIn(AttendanceStatus? status) =>
        status is AttendanceStatus.Present or AttendanceStatus.Late;

    public static string LocalTime(DateTime utc) =>
        DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime().ToString("d MMM yyyy, HH:mm");

    public static TimeOnly NowTime()
    {
        var now = DateTime.Now;
        return new TimeOnly(now.Hour, now.Minute);
    }
}
