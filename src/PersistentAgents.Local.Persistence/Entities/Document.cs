namespace PersistentAgents.Local.Persistence.Entities;

public class Document
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public Guid AgentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    // Navigation properties
    public Agent Agent { get; set; } = null!;
    public ICollection<Chunk> Chunks { get; set; } = new List<Chunk>();
}