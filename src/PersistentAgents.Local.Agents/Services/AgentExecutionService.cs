using PersistentAgents.Local.Core.Abstractions;
using PersistentAgents.Local.Core.Models;
using PersistentAgents.Local.Core.Services;
using System.Diagnostics;

namespace PersistentAgents.Local.Agents.Services;

public class AgentExecutionService : IAgentExecutionService
{
    private readonly IAgentService _agentService;
    private readonly IAgentRuntime _agentRuntime;
    private readonly IConversationService _conversationService;
    public AgentExecutionService(
        IAgentService agentService,
        IAgentRuntime agentRuntime,
        IConversationService conversationService)
    {
        _agentService = agentService;
        _agentRuntime = agentRuntime;
        _conversationService = conversationService;
    }
    public async Task<AgentExecutionResult> ExecuteAsync(
        AgentExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var stopwatch = Stopwatch.StartNew();

        var result = new AgentExecutionResult
        {
            AgentId = request.AgentId,
            ConversationId = request.ConversationId
        };

        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                throw new ArgumentException(
                    "Message cannot be empty.",
                    nameof(request));
            }

            var agent = await _agentService.GetAgentAsync(
                request.AgentId,
                cancellationToken);

            if (agent is null)
            {
                throw new InvalidOperationException(
                    $"Agent '{request.AgentId}' was not found.");
            }

            Console.WriteLine();
            Console.WriteLine("=== Agent Execution Diagnostics ===");
            Console.WriteLine($"Agent: {agent.Name}");
            Console.WriteLine($"Agent type: {agent.AgentType}");
            Console.WriteLine($"Agent ID: {agent.Id}");

            ConversationDefinition conversation;

            if (string.IsNullOrWhiteSpace(request.ConversationId))
            {
                conversation = await _conversationService
                    .CreateConversationAsync(
                        request.AgentId,
                        cancellationToken);
            }
            else
            {
                conversation = await _conversationService
                    .GetConversationAsync(
                        request.ConversationId,
                        cancellationToken)
                    ?? throw new InvalidOperationException(
                        $"Conversation '{request.ConversationId}' was not found.");

                if (conversation.AgentId != request.AgentId)
                {
                    throw new InvalidOperationException(
                        "The conversation does not belong to the specified agent.");
                }
            }

            result.ConversationId = conversation.Id;

            await _conversationService.AddMessageAsync(
                conversation.Id,
                "user",
                request.Message,
                cancellationToken);

            var conversationHistory =
                await _conversationService.GetMessagesAsync(
                    conversation.Id,
                    cancellationToken);

            Console.WriteLine(
                $"Conversation history count: {conversationHistory.Count}");

            Console.WriteLine("Calling agent runtime...");

            result.Answer = await _agentRuntime.ExecuteAsync(
                agent,
                conversationHistory,
                cancellationToken);

            Console.WriteLine("Agent runtime returned.");

            Console.WriteLine(
                $"Answer is null: {result.Answer is null}");

            Console.WriteLine(
                $"Answer is empty or whitespace: {string.IsNullOrWhiteSpace(result.Answer)}");

            Console.WriteLine(
                $"Answer length: {result.Answer?.Length ?? 0}");

            if (!string.IsNullOrWhiteSpace(result.Answer))
            {
                Console.WriteLine("Answer preview:");
                Console.WriteLine(
                    result.Answer.Length > 200
                        ? result.Answer[..200]
                        : result.Answer);
            }

            Console.WriteLine("Saving assistant message...");

            if (string.IsNullOrWhiteSpace(result.Answer))
            {
                throw new InvalidOperationException(
                    "Agent runtime returned an empty response.");
            }

            await _conversationService.AddMessageAsync(
                conversation.Id,
                "assistant",
                result.Answer,
                cancellationToken);

            result.Success = true;
        }
        catch (OperationCanceledException) when (
            cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("=== Agent Execution Error ===");
            Console.WriteLine(ex);

            result.Success = false;
            result.ErrorMessage = ex.Message;
        }
        finally
        {
            stopwatch.Stop();
            result.ExecutionTime = stopwatch.Elapsed;
        }

        return result;
    }
}