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
        var results = await _retrievalService.SearchAsync(
            _agentId,
            query,
            topK,
            cancellationToken: cancellationToken);

        if (results.Count == 0)
            return "No relevant documents were found.";

        var response = new System.Text.StringBuilder();

        foreach (var result in results)
        {
            response.AppendLine(
                $"DocumentId: {result.Chunk.DocumentId}");

            response.AppendLine(
                $"ChunkIndex: {result.Chunk.Index}");

            response.AppendLine(
                $"Similarity: {result.Similarity:F4}");

            response.AppendLine(
                $"Content: {result.Chunk.Content}");

            response.AppendLine();
        }

        return response.ToString();
    }
}