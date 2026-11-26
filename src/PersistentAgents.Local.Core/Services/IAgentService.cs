using PersistentAgents.Local.Core.Models;

namespace PersistentAgents.Local.Core.Services;

public interface IAgentService
{
    Task<AgentDefinition> CreateAgentAsync(
        AgentDefinition definition,
        CancellationToken cancellationToken = default);
    Task<AgentDefinition?> GetAgentAsync(
        Guid id,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AgentDefinition>> GetAllAgentsAsync(
        CancellationToken cancellationToken = default);
    Task<bool> UpdateAgentAsync(
        AgentDefinition definition,
        CancellationToken cancellationToken = default);
    Task<bool> DeleteAgentAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}