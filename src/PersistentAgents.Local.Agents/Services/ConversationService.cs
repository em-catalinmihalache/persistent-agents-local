using PersistentAgents.Local.Core.Models;
using PersistentAgents.Local.Core.Repositories;
using PersistentAgents.Local.Core.Services;

namespace PersistentAgents.Local.Agents.Services;

public class ConversationService : IConversationService
{
    private readonly IConversationRepository _repository;
    public ConversationService(IConversationRepository repository)
    {
        _repository = repository;
    }
    public Task<ConversationDefinition> CreateConversationAsync(
        Guid agentId,
        CancellationToken cancellationToken = default)
    {
        return _repository.CreateAsync(agentId, cancellationToken);
    }
    public Task<ConversationDefinition?> GetConversationAsync(
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        return _repository.GetByIdAsync(
            conversationId,
            cancellationToken);
    }
    public Task<ConversationMessage> AddMessageAsync(
        string conversationId,
        string role,
        string content,
        CancellationToken cancellationToken = default)
    {
        return _repository.AddMessageAsync(
            conversationId,
            role,
            content,
            cancellationToken);
    }
    public Task<IReadOnlyList<ConversationMessage>> GetMessagesAsync(
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        return _repository.GetMessagesAsync(
            conversationId,
            cancellationToken);
    }
}