namespace PersistentAgents.Local.Persistence.Entities;

public class Chunk
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DocumentId { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Embedding { get; set; } = string.Empty;
    // Navigation property
    public Document Document { get; set; } = null!;
}