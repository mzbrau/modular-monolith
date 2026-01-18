using TicketSystem.Testing.Common.Database;
using TicketSystem.Testing.Common.Factory;

namespace TicketSystem.Testing.Common.Fixtures;

/// <summary>
/// Base class for integration tests that use the shared fixture.
/// </summary>
public abstract class ModuleTestBase
{
    protected TicketSystemWebApplicationFactory Factory => IntegrationTestFixture.Factory;
    protected TestDatabaseManager DatabaseManager => IntegrationTestFixture.DatabaseManager;
    
    /// <summary>
    /// Override to specify cleanup strategy for this test class.
    /// </summary>
    protected virtual DatabaseCleanupStrategy CleanupStrategy => DatabaseCleanupStrategy.BeforeTestClass;

    [OneTimeSetUp]
    public virtual async Task TestClassSetUp()
    {
        if (CleanupStrategy == DatabaseCleanupStrategy.BeforeTestClass)
        {
            await DatabaseManager.CleanDatabaseAsync();
        }
    }

    [SetUp]
    public virtual async Task TestSetUp()
    {
        if (CleanupStrategy == DatabaseCleanupStrategy.BeforeEachTest)
        {
            await DatabaseManager.CleanDatabaseAsync();
        }
    }

    /// <summary>
    /// Gets a service from the DI container.
    /// </summary>
    protected T GetService<T>() where T : notnull
    {
        return Factory.GetRequiredService<T>();
    }

    /// <summary>
    /// Updates application settings during the test.
    /// </summary>
    protected void UpdateSettings()
    {
        Factory.UpdateSettings();
    }
}
