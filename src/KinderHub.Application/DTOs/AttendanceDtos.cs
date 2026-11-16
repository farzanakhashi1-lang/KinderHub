using System.ComponentModel.DataAnnotations;
using KinderHub.Domain.Enums;

namespace KinderHub.Application.DTOs;

public record AttendanceDto(
    int Id, int ChildId, string ChildName, DateOnly Date, AttendanceStatus Status,
    TimeOnly? CheckInTime, TimeOnly? CheckOutTime, string? PickedUpBy, string? Note);

// One row of the teacher's daily attendance sheet (record may not exist yet)
public class AttendanceRowDto
{
    public int ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public int? RecordId { get; set; }
    public AttendanceStatus? Status { get; set; }
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public string? PickedUpBy { get; set; }
    public string? Note { get; set; }
}

public class AttendanceEditModel : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int ChildId { get; set; }

    [Required]
    public DateOnly? Date { get; set; }

    [Required]
    public AttendanceStatus? Status { get; set; }

    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }

    [StringLength(100)]
    public string? PickedUpBy { get; set; }

    [StringLength(300)]
    public string? Note { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Date is { } d && d > DateOnly.FromDateTime(DateTime.Today))
            yield return new ValidationResult("Attendance cannot be marked for a future date.", new[] { nameof(Date) });

        if (Status is AttendanceStatus.Absent or AttendanceStatus.Sick && (CheckInTime is not null || CheckOutTime is not null))
            yield return new ValidationResult("An absent or sick child cannot have check-in/check-out times.", new[] { nameof(CheckInTime) });

        if (CheckOutTime is not null && CheckInTime is null)
            yield return new ValidationResult("Check-in time is required before check-out.", new[] { nameof(CheckInTime) });

        if (CheckInTime is { } inT && CheckOutTime is { } outT && outT <= inT)
            yield return new ValidationResult("Check-out must be after check-in.", new[] { nameof(CheckOutTime) });
    }
}
