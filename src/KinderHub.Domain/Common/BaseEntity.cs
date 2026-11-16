namespace KinderHub.Domain.Common;

// Every table has an Id and the date it was created.
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
