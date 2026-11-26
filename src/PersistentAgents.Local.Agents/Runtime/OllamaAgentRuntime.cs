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
            isRagAgent ? null : _retrievalService,
            includeDocumentSearchTool: !isRagAgent);

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

        if (isRagAgent)
        {
            var latestUserMessage = conversationHistory.LastOrDefault(message =>
                string.Equals(
                    message.Role,
                    "user",
                    StringComparison.OrdinalIgnoreCase));

            if (latestUserMessage is null)
            {
                throw new InvalidOperationException(
                    "A RAG execution requires a user question.");
            }

            var retrievedChunks = await _retrievalService.SearchAsync(
                agent.Id,
                latestUserMessage.Content,
                topK: 5,
                cancellationToken: cancellationToken);

            if (retrievedChunks.Count == 0)
            {
                return "I'm sorry, but the uploaded document does not contain the necessary information to answer that question.";
            }

            var context = string.Join(
                "\n\n",
                retrievedChunks.Select((result, index) =>
                    $"[Source {index + 1}: document chunk {result.Chunk.Index}]\n" +
                    result.Chunk.Content));

            messages.Insert(
                0,
                new ChatMessage(
                    ChatRole.System,
                    $"""
                    Retrieved document context for the current question:

                    {context}

                    Use only these retrieved passages to answer the latest user question.
                    Cite supporting passages with their exact [Source N] label.
                    If none of the passages answer the question, use the configured refusal response.
                    """));
        }

        var response = await chatAgent.RunAsync(
            messages,
            cancellationToken: cancellationToken);

        return response.ToString();
    }
}