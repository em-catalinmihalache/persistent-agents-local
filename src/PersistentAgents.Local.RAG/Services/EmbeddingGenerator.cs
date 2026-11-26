using System.Net.Http.Json;
using PersistentAgents.Local.Core.Abstractions;

namespace PersistentAgents.Local.RAG.Services;

public class EmbeddingGenerator : IEmbeddingGenerator
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    public EmbeddingGenerator(
        HttpClient httpClient,
        string model = "nomic-embed-text")
    {
        _httpClient = httpClient;
        _model = model;
    }
    public async Task<float[]> GenerateAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var request = new
        {
            model = _model,
            input = text
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "api/embed",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(
            cancellationToken: cancellationToken);

        if (result?.Embeddings is not { Length: > 0 })
        {
            throw new InvalidOperationException(
                "Ollama returned an empty embedding.");
        }

        return result.Embeddings[0];
    }
    private sealed class EmbeddingResponse
    {
        public float[][] Embeddings { get; set; } = [];
    }
}