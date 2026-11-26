namespace PersistentAgents.Local.Core.Models;

public class ConversationDefinition
{
    public string Id { get; set; } = string.Empty;
    public Guid AgentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<ConversationMessage> Messages { get; set; }
        = Array.Empty<ConversationMessage>();
}