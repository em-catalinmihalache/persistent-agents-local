using System.ComponentModel;

using PersistentAgents.Local.RAG.Services;

namespace PersistentAgents.Local.RAG.Tools;

public class SearchDocumentsTool
{
    private const int DefaultTopK = 5;
    private readonly Guid _agentId;
    private readonly VectorRetrievalService _retrievalService;    
    public SearchDocumentsTool(
        Guid agentId,
        VectorRetrievalService retrievalService)
    {
        _agentId = agentId;

        ArgumentNullException.ThrowIfNull(retrievalService);
        _retrievalService = retrievalService;
    }
    [Description(
        "Searches indexed documents and returns relevant passages. " +
        "You must use this tool to retrieve document information before answering questions.")]
    public async Task<string> SearchDocuments(
        [Description("The user's question or search query.")]
        string query,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        Console.WriteLine();
        Console.WriteLine("=== [TOOL CALLED] SearchDocuments ===");
        Console.WriteLine($"Agent ID: {_agentId}");
        Console.WriteLine($"Query: {query}");
        Console.WriteLine($"Top K: {DefaultTopK}");

        var results = await _retrievalService.SearchAsync(
            _agentId,
            query,
            DefaultTopK,
            cancellationToken: cancellationToken);

        Console.WriteLine($"Retrieved chunks: {results.Count}");
        Console.WriteLine();
        Console.WriteLine("=== Retrieval Similarity Scores ===");

        foreach (var result in results)
        {
            Console.WriteLine(
                $"Chunk {result.Chunk.Index}: Similarity = {result.Similarity:F4}");
        }

        if (results.Count == 0)
        {
            Console.WriteLine("=== [TOOL RESULT] No relevant documents found. ===");
            return "No relevant documents were found.";
        }

        var searchResult = string.Join(
            "\n\n",
            results.Select((result, index) =>
                $"[Source {index + 1}: document chunk {result.Chunk.Index}]\n" +
                result.Chunk.Content));

        Console.WriteLine();
        Console.WriteLine("=== SearchDocuments Returned Result ===");
        Console.WriteLine(searchResult);
        Console.WriteLine("=== End SearchDocuments Result ===");

        Console.WriteLine("=== [TOOL RESULT] Search completed successfully. ===");

        return searchResult;
    }
}