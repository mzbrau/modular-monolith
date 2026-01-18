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
        await session.CreateSQLQuery("DELETE FROM Issues").ExecuteUpdateAsync();
        
        // TeamMembers links Teams and Users
        await session.CreateSQLQuery("DELETE FROM TeamMembers").ExecuteUpdateAsync();
        
        // Then Teams and Users can be deleted
        await session.CreateSQLQuery("DELETE FROM Teams").ExecuteUpdateAsync();
        await session.CreateSQLQuery("DELETE FROM Users").ExecuteUpdateAsync();
        
        await transaction.CommitAsync();
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
