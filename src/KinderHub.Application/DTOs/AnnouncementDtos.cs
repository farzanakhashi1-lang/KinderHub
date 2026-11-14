using System.ComponentModel.DataAnnotations;

namespace KinderHub.Application.DTOs;

public record AnnouncementDto(
    int Id, string Title, string Message, bool IsImportant, string AuthorName,
    int? GroupId, string? GroupName, DateTime CreatedAt);

public class AnnouncementEditModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(150, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required.")]
    [StringLength(2000, MinimumLength = 5)]
    public string Message { get; set; } = string.Empty;

    public bool IsImportant { get; set; }

    // null = for everyone
    public int? GroupId { get; set; }

    public static AnnouncementEditModel FromDto(AnnouncementDto a) => new()
    {
        Id = a.Id, Title = a.Title, Message = a.Message, IsImportant = a.IsImportant, GroupId = a.GroupId
    };
}
