namespace PersistentAgents.Local.Persistence.Entities;

public class Agent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public string AgentType { get; set; } = "General";
    public string Model { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public float? Temperature { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Navigation properties
    public ICollection<Thread> Threads { get; set; } = new List<Thread>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<Run> Runs { get; set; } = new List<Run>();
}