namespace PersistentAgents.Local.Core.Models;

public class AgentDefinition
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
}