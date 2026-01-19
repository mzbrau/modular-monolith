using Microsoft.Extensions.DependencyInjection;
using NHibernate;

namespace TicketSystem.Testing.Common.Database;

public class TestDatabaseManager
{
    private readonly IServiceProvider _serviceProvider;
    
    public TestDatabaseManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Cleans all data from the test database.
    /// Call this in test setup to ensure clean state.
    /// </summary>
    public async Task CleanDatabaseAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var session = scope.ServiceProvider.GetRequiredService<ISession>();
        
        using var transaction = session.BeginTransaction();
        
        // Delete in correct order to respect foreign keys
        // Issues must be deleted first (references Users and Teams)
        // Use try-catch to handle cases where tables don't exist yet
        await TryDeleteFromTableAsync(session, "Issues");
        await TryDeleteFromTableAsync(session, "TeamMembers");
        await TryDeleteFromTableAsync(session, "Teams");
        await TryDeleteFromTableAsync(session, "Users");
        
        await transaction.CommitAsync();
    }
    
    private static async Task TryDeleteFromTableAsync(ISession session, string tableName)
    {
        try
        {
            await session.CreateSQLQuery($"DELETE FROM {tableName}").ExecuteUpdateAsync();
        }
        catch
        {
            // Ignore any errors during cleanup - table might not exist yet
            // This is expected during initial test setup before database schema is created
        }
    }

    /// <summary>
    /// Resets auto-increment sequences (SQLite specific).
    /// </summary>
    public async Task ResetSequencesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var session = scope.ServiceProvider.GetRequiredService<ISession>();
        
        using var transaction = session.BeginTransaction();
        
        await session.CreateSQLQuery("DELETE FROM sqlite_sequence").ExecuteUpdateAsync();
        
        await transaction.CommitAsync();
    }
}
