using KinderHub.Domain.Common;

namespace KinderHub.Domain.Entities;

public class Announcement : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsImportant { get; set; }
    public string AuthorName { get; set; } = string.Empty;

    // null = for everyone; otherwise only for one group
    public int? GroupId { get; set; }
    public Group? Group { get; set; }
}
