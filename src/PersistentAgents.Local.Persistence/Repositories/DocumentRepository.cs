using PersistentAgents.Local.RAG.Repositories;
using PersistentAgents.Local.Persistence.Context;
using Microsoft.EntityFrameworkCore;

using CoreDocument = PersistentAgents.Local.RAG.Models.Document;
using CoreChunk = PersistentAgents.Local.RAG.Models.Chunk;

using EntityDocument = PersistentAgents.Local.Persistence.Entities.Document;
using EntityChunk = PersistentAgents.Local.Persistence.Entities.Chunk;

namespace PersistentAgents.Local.Persistence.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AgentsDbContext _context;
    public DocumentRepository(AgentsDbContext context)
    {
        _context = context;
    }
    public async Task<CoreDocument?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Documents
            .Include(d => d.Chunks)
            .FirstOrDefaultAsync(
                d => d.Id == id.ToString(),
                cancellationToken);

        return entity is null ? null : ToModel(entity);
    }
    public async Task AddAsync(
        CoreDocument document,
        Guid agentId,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var entity = ToEntity(document);
        entity.AgentId = agentId;
        entity.FilePath = filePath;

        await _context.Documents.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task UpdateAsync(
        CoreDocument document,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Documents
            .Include(d => d.Chunks)
            .FirstOrDefaultAsync(
                d => d.Id == document.Id.ToString(),
                cancellationToken);

        if (entity is null)
            throw new KeyNotFoundException(
                $"Document with ID '{document.Id}' was not found.");

        entity.FileName = document.FileName;
        entity.UploadedAt = document.CreatedAt;

        _context.Chunks.RemoveRange(entity.Chunks);

        entity.Chunks = document.Chunks
            .Select(ToEntity)
            .ToList();

        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Documents
            .FirstOrDefaultAsync(
                d => d.Id == id.ToString(),
                cancellationToken);

        if (entity is null)
            return;

        _context.Documents.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<CoreChunk>> GetChunksWithEmbeddingsAsync(
        Guid agentId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.Chunks
            .AsNoTracking()
            .Where(c =>
                c.Document.AgentId == agentId &&
                c.Embedding != string.Empty)
            .OrderBy(c => c.DocumentId)
            .ThenBy(c => c.ChunkIndex)
            .ToListAsync(cancellationToken);

        return entities
            .Select(ToModel)
            .ToList();
    }
    private static CoreDocument ToModel(EntityDocument entity)
    {
        return new CoreDocument
        {
            Id = Guid.Parse(entity.Id),
            FileName = entity.FileName,
            CreatedAt = entity.UploadedAt,
            Chunks = entity.Chunks
                .OrderBy(c => c.ChunkIndex)
                .Select(ToModel)
                .ToList()
        };
    }
    private static CoreChunk ToModel(EntityChunk entity)
    {
        return new CoreChunk
        {
            Id = Guid.Parse(entity.Id),
            DocumentId = Guid.Parse(entity.DocumentId),
            Index = entity.ChunkIndex,
            Content = entity.Content,
            Embedding = entity.Embedding
        };
    }
    private static EntityDocument ToEntity(CoreDocument model)
    {
        return new EntityDocument
        {
            Id = model.Id.ToString(),
            FileName = model.FileName,
            UploadedAt = model.CreatedAt,
            Chunks = model.Chunks
                .Select(ToEntity)
                .ToList()
        };
    }
    private static EntityChunk ToEntity(CoreChunk model)
    {
        return new EntityChunk
        {
            Id = model.Id.ToString(),
            DocumentId = model.DocumentId.ToString(),
            ChunkIndex = model.Index,
            Content = model.Content,
            Embedding = model.Embedding ?? string.Empty
        };
    }
}