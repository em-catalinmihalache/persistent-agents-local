namespace PersistentAgents.Local.Core.Models;

public class AgentExecutionRequest
{
    public Guid AgentId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ConversationId { get; set; }
}