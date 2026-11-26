using PersistentAgents.Local.RAG.Models;

namespace PersistentAgents.Local.RAG.Services;

public class DocumentLoader
{
    public async Task<Document> LoadAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(
                "Document not found.", filePath);

        var content = await File.ReadAllTextAsync(
            filePath,
            cancellationToken);

        return new Document
        {
            FileName = Path.GetFileName(filePath),
            Content = content
        };
    }
}