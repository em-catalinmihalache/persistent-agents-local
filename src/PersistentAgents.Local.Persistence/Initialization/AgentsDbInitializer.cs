using PersistentAgents.Local.Persistence.Context;

namespace PersistentAgents.Local.Persistence.Initialization;

public static class AgentsDbInitializer
{
    public static async Task InitializeAsync(
        AgentsDbContext context)
    {
        await context.Database.EnsureCreatedAsync();
    }
}