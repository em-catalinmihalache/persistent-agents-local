namespace PersistentAgents.Local.RAG.Models;

public class Chunk
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DocumentId { get; set; }
    public int Index { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Embedding { get; set; }
    public Document? Document { get; set; }
}