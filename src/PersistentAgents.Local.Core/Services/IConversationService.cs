using PersistentAgents.Local.Core.Models;

namespace PersistentAgents.Local.Core.Services;

public interface IConversationService
{
    Task<ConversationDefinition> CreateConversationAsync(
        Guid agentId,
        CancellationToken cancellationToken = default);
    Task<ConversationDefinition?> GetConversationAsync(
        string conversationId,
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