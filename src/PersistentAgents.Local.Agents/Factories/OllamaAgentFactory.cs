using PersistentAgents.Local.Agents.Tools;
using PersistentAgents.Local.Core.Models;
using PersistentAgents.Local.RAG.Services;
using PersistentAgents.Local.RAG.Tools;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace PersistentAgents.Local.Agents.Factories;

public static class OllamaAgentFactory
{
    public static ChatClientAgent Create(
        string endpoint,
        AgentDefinition definition,
        VectorRetrievalService? retrievalService = null,
        bool includeDocumentSearchTool = true)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (!string.Equals(
            definition.Provider,
            "Ollama",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException(
                $"Provider '{definition.Provider}' is not supported.");
        }

        if (string.IsNullOrWhiteSpace(definition.Model))
        {
            throw new ArgumentException(
                "Agent model is required.",
                nameof(definition));
        }

        var chatClient = OllamaChatClientFactory.Create(
            endpoint,
            model: definition.Model);

        var tools = new List<AITool>();

        switch (definition.AgentType.ToLowerInvariant())
        {
            case "general":
                tools.Add(
                    AIFunctionFactory.Create(
                        CurrentDateTimeTool.GetCurrentDateTime));

                tools.Add(
                    AIFunctionFactory.Create(
                        CurrentDateTimeTool.GetCurrentDate));

                tools.Add(
                    AIFunctionFactory.Create(
                        CurrentDateTimeTool.GetCurrentTime));

                tools.Add(
                    AIFunctionFactory.Create(
                        WeatherOptionsTool.GetWeather));
                break;

            case "calculator":
                tools.Add(
                    AIFunctionFactory.Create(
                        CalculatorTool.Divide));

                tools.Add(
                    AIFunctionFactory.Create(
                        CalculatorTool.Multiply));
                break;

            case "rag":
                if (includeDocumentSearchTool && retrievalService is null)
                {
                    throw new ArgumentNullException(
                        nameof(retrievalService),
                        "RAG agents require VectorRetrievalService.");
                }

                if (includeDocumentSearchTool)
                {
                    var searchDocumentsTool = new SearchDocumentsTool(
                        retrievalService!,
                        definition.Id);

                    tools.Add(
                        AIFunctionFactory.Create(
                            searchDocumentsTool.SearchDocumentsAsync));
                }
                break;

            default:
                throw new NotSupportedException(
                    $"Agent type '{definition.AgentType}' is not supported.");
        }

        var instructions = string.IsNullOrWhiteSpace(
            definition.Instructions)
            ? "You are a helpful AI assistant. Provide clear and concise answers."
            : definition.Instructions;

        return chatClient.AsAIAgent(
            new ChatClientAgentOptions
            {
                Name = string.IsNullOrWhiteSpace(definition.Name)
                    ? "AgenticRAG Assistant"
                    : definition.Name,

                ChatOptions = new ChatOptions
                {
                    Temperature = definition.Temperature,
                    // Ollama's OpenAI-compatible API does not support tool_choice=required.
                    ToolMode = ChatToolMode.Auto,
                    Tools = tools,
                    Instructions = instructions
                }
            });
    }
}