using PersistentAgents.Local.Core.Models;
using PersistentAgents.Local.Core.Repositories;
using PersistentAgents.Local.Persistence.Context;
using PersistentAgents.Local.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace PersistentAgents.Local.Persistence.Repositories;

public class AgentRepository : IAgentRepository
{
    private readonly AgentsDbContext _context;
    public AgentRepository(AgentsDbContext context)
    {
        _context = context;
    }
    public async Task<AgentDefinition> AddAsync(
        AgentDefinition definition,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var entity = ToEntity(definition);

        await _context.Agents.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDefinition(entity);
    }
    public async Task<AgentDefinition?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Agents
            .AsNoTracking()
            .FirstOrDefaultAsync(
                a => a.Id == id,
                cancellationToken);

        return entity is null ? null : ToDefinition(entity);
    }
    public async Task<IReadOnlyList<AgentDefinition>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.Agents
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToDefinition).ToList();
    }
    public async Task<bool> UpdateAsync(
        AgentDefinition definition,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var entity = await _context.Agents
            .FirstOrDefaultAsync(
                a => a.Id == definition.Id,
                cancellationToken);

        if (entity is null)
            return false;

        entity.Name = definition.Name;
        entity.Description = definition.Description;
        entity.Instructions = definition.Instructions;
        entity.AgentType = definition.AgentType;
        entity.Model = definition.Model;
        entity.Provider = definition.Provider;
        entity.Temperature = definition.Temperature;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Agents
            .FirstOrDefaultAsync(
                a => a.Id == id,
                cancellationToken);

        if (entity is null)
            return false;

        _context.Agents.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
    private static Agent ToEntity(AgentDefinition definition)
    {
        return new Agent
        {
            Id = definition.Id,
            Name = definition.Name,
            Description = definition.Description,
            Instructions = definition.Instructions,
            AgentType = definition.AgentType,
            Model = definition.Model,
            Provider = definition.Provider,
            Temperature = definition.Temperature,
            CreatedAt = definition.CreatedAt
        };
    }
    private static AgentDefinition ToDefinition(Agent entity)
    {
        return new AgentDefinition
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Instructions = entity.Instructions,
            AgentType = entity.AgentType,
            Model = entity.Model,
            Provider = entity.Provider,
            Temperature = entity.Temperature,
            CreatedAt = entity.CreatedAt
        };
    }
}