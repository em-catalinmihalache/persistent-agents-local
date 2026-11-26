using PersistentAgents.Local.Core.Models;
using PersistentAgents.Local.Core.Repositories;
using PersistentAgents.Local.Core.Services;

namespace PersistentAgents.Local.Agents.Services;

public class AgentService : IAgentService
{
    private readonly IAgentRepository _repository;
    public AgentService(IAgentRepository repository)
    {
        _repository = repository;
    }
    public async Task<AgentDefinition> CreateAgentAsync(
        AgentDefinition definition,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        ValidateDefinition(definition);

        return await _repository.AddAsync(
            definition,
            cancellationToken);
    }
    public Task<AgentDefinition?> GetAgentAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _repository.GetByIdAsync(id, cancellationToken);
    }
    public Task<IReadOnlyList<AgentDefinition>> GetAllAgentsAsync(
        CancellationToken cancellationToken = default)
    {
        return _repository.GetAllAsync(cancellationToken);
    }
    public async Task<bool> UpdateAgentAsync(
        AgentDefinition definition,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        ValidateDefinition(definition);

        return await _repository.UpdateAsync(
            definition,
            cancellationToken);
    }
    public Task<bool> DeleteAgentAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(id, cancellationToken);
    }
    private static void ValidateDefinition(AgentDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(definition.Name))
            throw new ArgumentException("Agent name is required.");

        if (string.IsNullOrWhiteSpace(definition.Instructions))
            throw new ArgumentException("Agent instructions are required.");

        if (string.IsNullOrWhiteSpace(definition.Model))
            throw new ArgumentException("Agent model is required.");

        if (string.IsNullOrWhiteSpace(definition.Provider))
            throw new ArgumentException("Agent provider is required.");

        if (definition.Temperature is < 0 or > 2)
            throw new ArgumentOutOfRangeException(
                nameof(definition.Temperature),
                "Temperature must be between 0 and 2.");
    }
}