using NUnit.Framework;
using TicketSystem.Testing.Common.Database;
using TicketSystem.Testing.Common.Factory;

namespace TicketSystem.Testing.Common.Fixtures;

/// <summary>
/// Global fixture that manages the WebApplicationFactory lifecycle.
/// Shared across all tests in a test assembly.
/// </summary>
[SetUpFixture]
public abstract class IntegrationTestFixture
{
    private static TicketSystemWebApplicationFactory? _factory;
    private static TestDatabaseManager? _databaseManager;

    public static TicketSystemWebApplicationFactory Factory => 
        _factory ?? throw new InvalidOperationException("Test fixture not initialized");
    
    public static TestDatabaseManager DatabaseManager => 
        _databaseManager ?? throw new InvalidOperationException("Test fixture not initialized");

    [OneTimeSetUp]
    public virtual async Task GlobalSetUp()
    {
        _factory = new TicketSystemWebApplicationFactory();
        
        // Ensure the factory is fully initialized by creating a client
        var client = _factory.CreateClient();
        
        // Make a request to ensure the application has fully started and database is initialized
        // This triggers the Program.cs startup code including database initialization
        try
        {
            await client.GetAsync("/_health");
        }
        catch
        {
            // Ignore any errors from the health check - we just need to trigger startup
        }
        
        // Ensure database schema is initialized before any operations
        _factory.EnsureDatabaseInitialized();
        
        _databaseManager = new TestDatabaseManager(_factory.Services);
        
        // Initial database cleanup
        await _databaseManager.CleanDatabaseAsync();
    }

    [OneTimeTearDown]
    public virtual void GlobalTearDown()
    {
        _factory?.Dispose();
        _factory = null;
        _databaseManager = null;
    }
}
