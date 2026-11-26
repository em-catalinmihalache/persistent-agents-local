using PersistentAgents.Local.Core.Models;

namespace PersistentAgents.Local.Core.Repositories;

public interface IConversationRepository
{
    Task<ConversationDefinition> CreateAsync(
        Guid agentId,
        CancellationToken cancellationToken = default);
    Task<ConversationDefinition?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);
    Task<ConversationMessage> AddMessageAsync(
        string conversationId,
        string role,
        string content,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConversationMessage>> GetMessagesAsync(
        string conversationId,
        CancellationToken cancellationToken = default);
}