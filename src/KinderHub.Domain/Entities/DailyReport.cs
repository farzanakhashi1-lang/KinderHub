using KinderHub.Domain.Common;
using KinderHub.Domain.Enums;

namespace KinderHub.Domain.Entities;

public class DailyReport : BaseEntity
{
    public int ChildId { get; set; }
    public Child Child { get; set; } = null!;

    public DateOnly Date { get; set; }
    public Mood Mood { get; set; }
    public string Meals { get; set; } = string.Empty;
    public int NapMinutes { get; set; }
    public string Activities { get; set; } = string.Empty;
    public string? TeacherComment { get; set; }
    public string AuthorName { get; set; } = string.Empty;
}
