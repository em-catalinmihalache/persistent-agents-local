using System.Text.Json;
using PersistentAgents.Local.Core.Abstractions;
using PersistentAgents.Local.RAG.Repositories;

namespace PersistentAgents.Local.RAG.Services;

public class VectorPersistenceService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IEmbeddingGenerator _embeddingGenerator;
    public VectorPersistenceService(
        IDocumentRepository documentRepository,
        IEmbeddingGenerator embeddingGenerator)
    {
        _documentRepository = documentRepository;
        _embeddingGenerator = embeddingGenerator;
    }
    public async Task ProcessAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        // 1. Retrieve document and chunks
        var document = await _documentRepository.GetByIdAsync(
            documentId,
            cancellationToken);

        if (document is null)
        {
            throw new InvalidOperationException(
                $"Document {documentId} was not found.");
        }

        // 2. Generate embeddings for each chunk
        foreach (var chunk in document.Chunks)
        {
            var embedding = await _embeddingGenerator.GenerateAsync(
                chunk.Content,
                cancellationToken);

            // 3. Serialize embedding to JSON
            chunk.Embedding = JsonSerializer.Serialize(embedding);
        }

        // 4. Persist updated document
        await _documentRepository.UpdateAsync(
            document,
            cancellationToken);
    }
}