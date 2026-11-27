using PersistentAgents.Local.RAG.Services;

namespace PersistentAgents.Local.RAG.Tools;

public class SearchDocumentsTool
{
    private readonly VectorRetrievalService _retrievalService;
    private readonly Guid _agentId;
    public SearchDocumentsTool(
        VectorRetrievalService retrievalService,
        Guid agentId)
    {
        _retrievalService = retrievalService;
        _agentId = agentId;
    }
    public async Task<string> SearchDocumentsAsync(
        string query,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("[Tool] SearchDocumentsAsync invoked.");

        var results = await _retrievalService.SearchAsync(
            _agentId,
            query,
            topK,
            cancellationToken: cancellationToken);

        Console.WriteLine($"[Tool] Retrieved {results.Count} chunk(s).");

        if (results.Count == 0)
            return "No relevant documents were found.";

        return string.Join(
            "\n\n",
            results.Select((result, index) =>
                $"[Source {index + 1}: document chunk {result.Chunk.Index}]\n" +
                result.Chunk.Content));
    }
}