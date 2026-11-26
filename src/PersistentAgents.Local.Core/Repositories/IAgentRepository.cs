using PersistentAgents.Local.Core.Models;

namespace PersistentAgents.Local.Core.Repositories;

public interface IAgentRepository
{
    Task<AgentDefinition> AddAsync(
        AgentDefinition definition,
        CancellationToken cancellationToken = default);
    Task<AgentDefinition?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AgentDefinition>> GetAllAsync(
        CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(
        AgentDefinition definition,
        CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}