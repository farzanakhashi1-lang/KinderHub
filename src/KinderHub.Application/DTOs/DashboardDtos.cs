namespace KinderHub.Application.DTOs;

public record DailyAttendancePoint(DateOnly Date, int Present, int Absent);

public record GroupOccupancy(string GroupName, int Children, int Capacity);

public record DashboardStatsDto(
    int TotalChildren,
    int TotalGroups,
    int PresentToday,
    int AbsentToday,
    int NotMarkedToday,
    double AttendanceRate14Days,
    IReadOnlyList<DailyAttendancePoint> AttendanceTrend,
    IReadOnlyList<GroupOccupancy> Occupancy);
