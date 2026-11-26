namespace PersistentAgents.Local.Core.Models;

public class AgentExecutionResult
{
    public Guid AgentId { get; set; }
    public string Answer { get; set; } = string.Empty;
    public string? ConversationId { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan ExecutionTime { get; set; }
}