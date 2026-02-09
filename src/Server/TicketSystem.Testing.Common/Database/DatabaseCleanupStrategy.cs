namespace TicketSystem.Testing.Common.Database;

public enum DatabaseCleanupStrategy
{
    /// <summary>
    /// Clean database before each test class (faster, tests in class share data).
    /// </summary>
    BeforeTestClass,
    
    /// <summary>
    /// Clean database before each test (slower, full isolation).
    /// </summary>
    BeforeEachTest,
    
    /// <summary>
    /// No automatic cleanup (tests manage their own data).
    /// </summary>
    Manual
}
