using KinderHub.Domain.Common;
using KinderHub.Domain.Enums;

namespace KinderHub.Domain.Entities;

public class AttendanceRecord : BaseEntity
{
    public int ChildId { get; set; }
    public Child Child { get; set; } = null!;

    public DateOnly Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public string? PickedUpBy { get; set; }
    public string? Note { get; set; }
}
