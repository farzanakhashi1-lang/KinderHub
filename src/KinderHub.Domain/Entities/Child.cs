using KinderHub.Domain.Common;

namespace KinderHub.Domain.Entities;

public class Child : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }

    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public string ParentName { get; set; } = string.Empty;
    public string ParentEmail { get; set; } = string.Empty;
    public string ParentPhone { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    public ICollection<DailyReport> DailyReports { get; set; } = new List<DailyReport>();

    // Calculated (not saved in the database)
    public string FullName => $"{FirstName} {LastName}";

    public int GetAge(DateOnly today)
    {
        var age = today.Year - DateOfBirth.Year;
        if (DateOfBirth > today.AddYears(-age)) age--;
        return age;
    }
}
