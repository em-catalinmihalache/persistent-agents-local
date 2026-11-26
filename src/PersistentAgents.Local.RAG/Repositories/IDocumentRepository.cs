using PersistentAgents.Local.RAG.Models;

namespace PersistentAgents.Local.RAG.Repositories;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
    Task AddAsync(
        Document document,
        Guid agentId,
        string filePath,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(
        Document document,
        CancellationToken cancellationToken = default);
    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Chunk>> GetChunksWithEmbeddingsAsync(
        Guid agentId,
        CancellationToken cancellationToken = default);
}