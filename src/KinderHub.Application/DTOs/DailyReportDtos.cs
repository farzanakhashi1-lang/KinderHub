using System.ComponentModel.DataAnnotations;
using KinderHub.Domain.Enums;

namespace KinderHub.Application.DTOs;

public record DailyReportDto(
    int Id, int ChildId, string ChildName, DateOnly Date, Mood Mood, string Meals,
    int NapMinutes, string Activities, string? TeacherComment, string AuthorName);

public class DailyReportEditModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Please choose a child.")]
    public int ChildId { get; set; }

    [Required]
    public DateOnly? Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public Mood Mood { get; set; } = Mood.Happy;

    [Required(ErrorMessage = "Please describe the meals.")]
    [StringLength(300)]
    public string Meals { get; set; } = string.Empty;

    [Range(0, 240, ErrorMessage = "Nap must be between 0 and 240 minutes.")]
    public int NapMinutes { get; set; }

    [Required(ErrorMessage = "Please describe the activities.")]
    [StringLength(500)]
    public string Activities { get; set; } = string.Empty;

    [StringLength(500)]
    public string? TeacherComment { get; set; }

    public static DailyReportEditModel FromDto(DailyReportDto r) => new()
    {
        ChildId = r.ChildId, Date = r.Date, Mood = r.Mood, Meals = r.Meals,
        NapMinutes = r.NapMinutes, Activities = r.Activities, TeacherComment = r.TeacherComment
    };
}
