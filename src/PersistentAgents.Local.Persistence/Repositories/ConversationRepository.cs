using PersistentAgents.Local.Core.Models;
using PersistentAgents.Local.Core.Repositories;
using PersistentAgents.Local.Persistence.Context;
using PersistentAgents.Local.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

using AgentThread = PersistentAgents.Local.Persistence.Entities.Thread;

namespace PersistentAgents.Local.Persistence.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly AgentsDbContext _context;
    public ConversationRepository(AgentsDbContext context)
    {
        _context = context;
    }
    public async Task<ConversationDefinition> CreateAsync(
        Guid agentId,
        CancellationToken cancellationToken = default)
    {
        var agentExists = await _context.Agents
            .AnyAsync(a => a.Id == agentId, cancellationToken);

        if (!agentExists)
        {
            throw new InvalidOperationException(
                $"Agent '{agentId}' was not found.");
        }

        var thread = new AgentThread
        {
            Id = Guid.NewGuid().ToString(),
            AgentId = agentId,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Threads.AddAsync(thread, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDefinition(thread);
    }
    public async Task<ConversationDefinition?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var thread = await _context.Threads
            .AsNoTracking()
            .Include(t => t.Messages)
            .FirstOrDefaultAsync(
                t => t.Id == id,
                cancellationToken);

        return thread is null ? null : ToDefinition(thread);
    }
    public async Task<ConversationMessage> AddMessageAsync(
        string conversationId,
        string role,
        string content,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role cannot be empty.", nameof(role));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.", nameof(content));

        var threadExists = await _context.Threads
            .AnyAsync(t => t.Id == conversationId, cancellationToken);

        if (!threadExists)
        {
            throw new InvalidOperationException(
                $"Conversation '{conversationId}' was not found.");
        }

        var message = new Message
        {
            Id = Guid.NewGuid().ToString(),
            ThreadId = conversationId,
            Role = role,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Messages.AddAsync(message, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ToMessage(message);
    }
    public async Task<IReadOnlyList<ConversationMessage>> GetMessagesAsync(
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        var messages = await _context.Messages
            .AsNoTracking()
            .Where(m => m.ThreadId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        return messages.Select(ToMessage).ToList();
    }
    private static ConversationDefinition ToDefinition(
        AgentThread thread)
    {
        return new ConversationDefinition
        {
            Id = thread.Id,
            AgentId = thread.AgentId,
            CreatedAt = thread.CreatedAt,
            Messages = thread.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(ToMessage)
                .ToList()
        };
    }
    private static ConversationMessage ToMessage(Message message)
    {
        return new ConversationMessage
        {
            Id = message.Id,
            ThreadId = message.ThreadId,
            Role = message.Role,
            Content = message.Content,
            CreatedAt = message.CreatedAt
        };
    }
}