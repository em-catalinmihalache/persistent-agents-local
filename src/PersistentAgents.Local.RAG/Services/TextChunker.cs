using PersistentAgents.Local.RAG.Models;

namespace PersistentAgents.Local.RAG.Services;

public class TextChunker
{
    public List<Chunk> Chunk(Document document)
    {
        var chunks = new List<Chunk>();

        var lines = document.Content.Split(
            ['\r', '\n'],
            StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var content = line.Trim();

            if (!content.StartsWith("- "))
                continue;

            chunks.Add(new Chunk
            {
                DocumentId = document.Id,
                Index = chunks.Count,
                Content = content
            });
        }

        document.Chunks = chunks;

        return chunks;
    }
}