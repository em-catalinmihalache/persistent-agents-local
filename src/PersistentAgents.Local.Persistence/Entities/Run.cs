namespace PersistentAgents.Local.Persistence.Entities;

public class Run
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public Guid AgentId { get; set; }
    public string ThreadId { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    // Navigation properties
    public Agent Agent { get; set; } = null!;
    public Thread Thread { get; set; } = null!;
}