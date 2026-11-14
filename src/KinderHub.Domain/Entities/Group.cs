using KinderHub.Domain.Common;

namespace KinderHub.Domain.Entities;

public class Group : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string AgeRange { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string? TeacherName { get; set; }
    public int Capacity { get; set; } = 15;

    public ICollection<Child> Children { get; set; } = new List<Child>();
}
