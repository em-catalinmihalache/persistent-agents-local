namespace PersistentAgents.Local.Persistence.Entities;

public class Thread
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public Guid AgentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Navigation properties
    public Agent Agent { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<Run> Runs { get; set; } = new List<Run>();
}