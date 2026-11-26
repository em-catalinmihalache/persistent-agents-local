using PersistentAgents.Local.Core.Models;

namespace PersistentAgents.Local.Core.Services;

public interface IAgentExecutionService
{
    Task<AgentExecutionResult> ExecuteAsync(
        AgentExecutionRequest request,
        CancellationToken cancellationToken = default);
}