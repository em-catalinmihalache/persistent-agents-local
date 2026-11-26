using PersistentAgents.Local.Core.Models;

namespace PersistentAgents.Local.Core.Abstractions;

public interface IAgentRuntime
{
    Task<string> ExecuteAsync(
        AgentDefinition agent,
        IReadOnlyList<ConversationMessage> conversationHistory,
        CancellationToken cancellationToken = default);
}