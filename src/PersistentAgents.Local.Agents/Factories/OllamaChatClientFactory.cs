using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace PersistentAgents.Local.Agents.Factories;

public static class OllamaChatClientFactory
{
    public static IChatClient Create(
        string model = "qwen2.5:3b",
        string endpoint = "http://localhost:11434/v1")
    {
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(endpoint)
        };

        var openAIClient = new OpenAIClient(
            new ApiKeyCredential("ollama"),
            options);

        return openAIClient
            .GetChatClient(model)
            .AsIChatClient();
    }
}