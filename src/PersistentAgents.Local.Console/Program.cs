using PersistentAgents.Local.Agents.Runtime;
using PersistentAgents.Local.Agents.Services;
using PersistentAgents.Local.Core.Abstractions;
using PersistentAgents.Local.Core.Models;
using PersistentAgents.Local.Core.Repositories;
using PersistentAgents.Local.Core.Services;
using PersistentAgents.Local.Persistence.Context;
using PersistentAgents.Local.Persistence.Entities;
using PersistentAgents.Local.Persistence.Initialization;
using PersistentAgents.Local.Persistence.Repositories;
using PersistentAgents.Local.RAG.Repositories;
using PersistentAgents.Local.RAG.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

const string agentName = "DotNetRAGAgent";
const string instructions = """
    You are a document retrieval assistant.

    You have access to a tool named SearchDocuments.

    For every new user message about documents, you MUST call SearchDocuments
    before generating an answer.

    This applies to every follow-up question, even when previous assistant
    messages contain relevant information.

    Previous assistant messages are conversational context only.
    They are not document evidence and must not replace a new search.

    Do not answer from your own knowledge.
    Do not generate an answer before receiving the tool result.

    After receiving the SearchDocuments result:

    - Use only the information returned by SearchDocuments.
    - Cite supporting passages using their exact [Source N] labels.
    - If SearchDocuments returns "No relevant documents were found.",
      respond exactly:
      "I'm sorry, but the uploaded document does not contain the necessary information to answer that question."
    - If the retrieved context does not contain enough information to answer,
      use the same refusal response.
    """;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
builder.Logging.AddFilter("System.Net.Http.HttpClient.ollama", LogLevel.Warning);

var ollamaLocalEndpoint = builder.Configuration["OLLAMA:LOCAL_ENDPOINT"]
    ?? "http://localhost:11434";
var ollamaLargeLanguageModel = builder.Configuration["OLLAMA:LARGE_LANGUAGE_MODEL"]
    ?? "qwen2.5:3b";
var ollamaEmbeddingModel = builder.Configuration["OLLAMA:EMBEDDING_MODEL"]
    ?? "nomic-embed-text";

// Keep the database beside the SampleApp project, including when launched from bin/Debug.
var databasePath = Path.GetFullPath(Path.Combine(
    AppContext.BaseDirectory,
    "..", "..", "..",
    "agents.db"));

builder.Services.AddDbContext<AgentsDbContext>(options =>
    options.UseSqlite($"Data Source={databasePath}"));

builder.Services.AddScoped<IAgentRepository, AgentRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IAgentService, AgentService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<VectorPersistenceService>();
builder.Services.AddSingleton<CosineSimilarityCalculator>();
builder.Services.AddScoped<VectorRetrievalService>();
builder.Services.AddScoped<IAgentRuntime>(services => new OllamaAgentRuntime(
    services.GetRequiredService<VectorRetrievalService>(),
    new Uri(ollamaLocalEndpoint.TrimEnd('/') + "/v1").AbsoluteUri));
builder.Services.AddScoped<IAgentExecutionService, AgentExecutionService>();

builder.Services.AddHttpClient("ollama", client =>
    client.BaseAddress = new Uri(ollamaLocalEndpoint.TrimEnd('/') + "/"));
builder.Services.AddScoped<IEmbeddingGenerator>(services =>
    new EmbeddingGenerator(
        services.GetRequiredService<IHttpClientFactory>().CreateClient("ollama"),
        ollamaEmbeddingModel));

using var host = builder.Build();

await using (var scope = host.Services.CreateAsyncScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<AgentsDbContext>();
    await AgentsDbInitializer.InitializeAsync(dbContext);

    var agentService = services.GetRequiredService<IAgentService>();
    var agent = (await agentService.GetAllAgentsAsync())
        .FirstOrDefault(item => string.Equals(
            item.Name,
            agentName,
            StringComparison.OrdinalIgnoreCase));

    if (agent is null)
    {
        agent = await agentService.CreateAgentAsync(new AgentDefinition
        {
            Name = agentName,
            Description = "Answers questions using only the indexed document.md file.",
            Instructions = instructions,
            AgentType = "RAG",
            Model = ollamaLargeLanguageModel,
            Provider = "Ollama",
            Temperature = 0.2f
        });
    }
    else
    {
        agent.Description = "Answers questions using only the indexed document.md file.";
        agent.Instructions = instructions;
        agent.AgentType = "RAG";
        agent.Model = ollamaLargeLanguageModel;
        agent.Provider = "Ollama";
        agent.Temperature = 0.2f;
        await agentService.UpdateAgentAsync(agent);
    }

    // Index this document once per agent. Its content, chunks and embeddings are stored in agents.db.
    var documentPath = Path.Combine(AppContext.BaseDirectory, "Documents", "document.md");
    if (!File.Exists(documentPath))
    {
        throw new FileNotFoundException(
            "The sample document was not copied to the application output directory.",
            documentPath);
    }

    var documentAlreadyIndexed = await dbContext.Documents.AnyAsync(document =>
        document.AgentId == agent.Id && document.FileName == Path.GetFileName(documentPath));

    if (!documentAlreadyIndexed)
    {
        var loader = new DocumentLoader();
        var document = await loader.LoadAsync(documentPath);
        document.Chunks = new TextChunker().Chunk(document);

        await services.GetRequiredService<IDocumentRepository>().AddAsync(
            document,
            agent.Id,
            documentPath);

        await services.GetRequiredService<VectorPersistenceService>()
            .ProcessAsync(document.Id);
    }

    var conversation = await services.GetRequiredService<IConversationService>()
        .CreateConversationAsync(agent.Id);

    var executionService = services.GetRequiredService<IAgentExecutionService>();

    Console.WriteLine("=== Local AgenticRAG Sample ===");
    Console.WriteLine();
    Console.WriteLine($"Agent: {agent.Name} ({agent.Provider}, {agent.Model}, {ollamaEmbeddingModel})");
    Console.WriteLine($"Document: {Path.GetFileName(documentPath)}");
    Console.WriteLine($"Conversation thread: {conversation.Id}");
    Console.WriteLine();

    var questions = new[]
    {
        "What does Contoso's travel insurance cover?",
        "Does it also cover flight delays?"
    };

    for (int i = 0; i < questions.Length; i++)
    {
        string? currentQuestion = questions[i];
        var run = new Run
        {
            AgentId = agent.Id,
            ThreadId = conversation.Id,
            Status = "Running",
            StartedAt = DateTime.UtcNow
        };

        await dbContext.Runs.AddAsync(run);
        await dbContext.SaveChangesAsync();

        Console.WriteLine($"Question {i + 1}: {currentQuestion}");
        Console.WriteLine("Searching the indexed document and generating an answer with Ollama...");

        try
        {
            var execution = await executionService.ExecuteAsync(
                new AgentExecutionRequest
                {
                    AgentId = agent.Id,
                    ConversationId = conversation.Id,
                    Message = currentQuestion
                });

            run.CompletedAt = DateTime.UtcNow;
            run.Status = execution.Success ? "Completed" : "Failed";

            await dbContext.SaveChangesAsync();

            if (!execution.Success)
            {
                throw new InvalidOperationException(
                    execution.ErrorMessage ?? "Agent execution failed.");
            }

            Console.WriteLine();
            Console.WriteLine("=== Answer ===");
            Console.WriteLine(execution.Answer);
            Console.WriteLine();
            Console.WriteLine($"Run ID: {run.Id}");
            Console.WriteLine($"Run status: {run.Status}");
            //Console.WriteLine($"Conversation ID: {execution.ConversationId}");
            Console.WriteLine();
        }
        catch
        {
            run.CompletedAt = DateTime.UtcNow;
            run.Status = "Failed";
            await dbContext.SaveChangesAsync();
            throw;
        }
    }

    Console.WriteLine("=== Conversation Context Test Completed ===");
    Console.WriteLine($"Conversation ID: {conversation.Id}");
    Console.WriteLine($"Database: {databasePath}");
}

Console.WriteLine("\nPress ENTER to exit...");
Console.ReadLine();
