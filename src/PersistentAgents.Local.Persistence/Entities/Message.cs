namespace PersistentAgents.Local.Persistence.Entities;

public class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string ThreadId { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Thread Thread { get; set; } = null!;
}