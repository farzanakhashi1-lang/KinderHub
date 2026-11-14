using System.ComponentModel.DataAnnotations;

namespace KinderHub.Application.DTOs;

// What the pages SHOW
public record GroupDto(int Id, string Name, string AgeRange, string Room, string? TeacherName, int Capacity, int ChildrenCount);

// What the forms EDIT (with validation rules)
public class GroupEditModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Group name is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Age range is required.")]
    [StringLength(50)]
    public string AgeRange { get; set; } = string.Empty;

    [Required(ErrorMessage = "Room is required.")]
    [StringLength(50)]
    public string Room { get; set; } = string.Empty;

    [StringLength(100)]
    public string? TeacherName { get; set; }

    [Range(1, 40, ErrorMessage = "Capacity must be between 1 and 40.")]
    public int Capacity { get; set; } = 15;

    public static GroupEditModel FromDto(GroupDto g) => new()
    {
        Id = g.Id, Name = g.Name, AgeRange = g.AgeRange, Room = g.Room, TeacherName = g.TeacherName, Capacity = g.Capacity
    };
}
