using System.Text.Json;
using PersistentAgents.Local.Core.Abstractions;
using PersistentAgents.Local.Core.Models;
using PersistentAgents.Local.Core.Repositories;
using PersistentAgents.Local.RAG.Models;
using PersistentAgents.Local.RAG.Repositories;

namespace PersistentAgents.Local.RAG.Services;

public class VectorRetrievalService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IEmbeddingGenerator _embeddingGenerator;
    private readonly CosineSimilarityCalculator _similarityCalculator;
    public VectorRetrievalService(
        IDocumentRepository documentRepository,
        IEmbeddingGenerator embeddingGenerator,
        CosineSimilarityCalculator similarityCalculator)
    {
        _documentRepository = documentRepository;
        _embeddingGenerator = embeddingGenerator;
        _similarityCalculator = similarityCalculator;
    }
    public async Task<IReadOnlyList<RetrievedChunk>> SearchAsync(
        Guid agentId,
        string query,
        int topK = 5,
        double minSimilarity = 0.0,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException(
                "Query cannot be empty.", nameof(query));

        if (topK <= 0)
            throw new ArgumentOutOfRangeException(nameof(topK));

        if (minSimilarity < -1.0 || minSimilarity > 1.0)
            throw new ArgumentOutOfRangeException(nameof(minSimilarity));

        // 1. Generate query embedding
        var queryEmbedding = await _embeddingGenerator.GenerateAsync(
            query,
            cancellationToken);

        // 2. Retrieve chunks with embeddings
        var chunks = await _documentRepository.GetChunksWithEmbeddingsAsync(
            agentId,
            cancellationToken);

        var results = new List<RetrievedChunk>();

        // 3. Calculate similarity
        foreach (var chunk in chunks)
        {
            if (string.IsNullOrWhiteSpace(chunk.Embedding))
                continue;

            var embedding = JsonSerializer.Deserialize<float[]>(
                chunk.Embedding);

            if (embedding is null)
                continue;

            var similarity = _similarityCalculator.Calculate(
                queryEmbedding,
                embedding);

            if (similarity < minSimilarity)
                continue;

            results.Add(new RetrievedChunk
            {
                Chunk = chunk,
                Similarity = similarity
            });
        }

        // 4. Return Top-K results
        return results
            .OrderByDescending(r => r.Similarity)
            .Take(topK)
            .ToList();
    }
}