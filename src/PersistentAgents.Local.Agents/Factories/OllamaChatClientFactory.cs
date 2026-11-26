using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace PersistentAgents.Local.Agents.Factories;

public static class OllamaChatClientFactory
{
    public static IChatClient Create(
        string endpoint,
        string model)
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