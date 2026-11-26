namespace PersistentAgents.Local.RAG.Models;

public class RetrievedChunk
{
    public Chunk Chunk { get; set; } = null!;
    public double Similarity { get; set; }
}