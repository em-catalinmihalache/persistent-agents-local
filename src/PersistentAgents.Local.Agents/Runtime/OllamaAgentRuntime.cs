using PersistentAgents.Local.Agents.Factories;
using PersistentAgents.Local.Core.Abstractions;
using PersistentAgents.Local.Core.Models;
using PersistentAgents.Local.RAG.Services;

using Microsoft.Extensions.AI;

namespace PersistentAgents.Local.Agents.Runtime;

public class OllamaAgentRuntime : IAgentRuntime
{
    private readonly VectorRetrievalService _retrievalService;
    private readonly string _endpoint;
    public OllamaAgentRuntime(
        VectorRetrievalService retrievalService,
        string endpoint)
    {
        _retrievalService = retrievalService;
        _endpoint = endpoint;
    }
    public async Task<string> ExecuteAsync(
        AgentDefinition agent,
        IReadOnlyList<ConversationMessage> conversationHistory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(agent);
        ArgumentNullException.ThrowIfNull(conversationHistory);

        if (conversationHistory.Count == 0)
        {
            throw new ArgumentException(
                "Conversation history cannot be empty.",
                nameof(conversationHistory));
        }

        var isRagAgent = string.Equals(
            agent.AgentType,
            "RAG",
            StringComparison.OrdinalIgnoreCase);

        var chatAgent = OllamaAgentFactory.Create(
            _endpoint,
            agent,
            isRagAgent ? _retrievalService : null,
            includeDocumentSearchTool: isRagAgent);

        var messages = conversationHistory
            .Select(message => new ChatMessage(
                message.Role.ToLowerInvariant() switch
                {
                    "user" => ChatRole.User,
                    "assistant" => ChatRole.Assistant,
                    "system" => ChatRole.System,
                    _ => throw new ArgumentException(
                        $"Unsupported message role: {message.Role}")
                },
                message.Content))
            .ToList();

        var response = await chatAgent.RunAsync(
            messages,
            cancellationToken: cancellationToken);

        return response.ToString();
    }
}