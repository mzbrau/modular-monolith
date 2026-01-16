using System.Data;
using System.Data.SQLite;

namespace TicketSystem.Api.Database;

public class DatabaseInitializer
{
    private readonly DatabaseOptions _databaseOptions;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(DatabaseOptions databaseOptions, ILogger<DatabaseInitializer> logger)
    {
        _databaseOptions = databaseOptions;
        _logger = logger;
    }

    public void Initialize()
    {
        _logger.LogInformation("Initializing database schema...");
        
        try
        {
            using var connection = new SQLiteConnection(_databaseOptions.ConnectionString);
            connection.Open();

            ExecuteCreateTableStatements(connection);
            
            _logger.LogInformation("Database schema initialized successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing database schema.");
            throw;
        }
    }

    private void ExecuteCreateTableStatements(IDbConnection connection)
    {
        var createTableStatements = new[]
        {
            @"CREATE TABLE IF NOT EXISTS Users (
                Id TEXT PRIMARY KEY,
                Email TEXT NOT NULL UNIQUE,
                FirstName TEXT NOT NULL,
                LastName TEXT NOT NULL,
                CreatedDate TEXT NOT NULL,
                IsActive INTEGER NOT NULL DEFAULT 1
            )",
            @"CREATE TABLE IF NOT EXISTS Teams (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Description TEXT,
                CreatedDate TEXT NOT NULL
            )",
            @"CREATE TABLE IF NOT EXISTS TeamMembers (
                Id TEXT PRIMARY KEY,
                TeamId TEXT NOT NULL,
                UserId TEXT NOT NULL,
                JoinedDate TEXT NOT NULL,
                Role INTEGER NOT NULL,
                FOREIGN KEY (TeamId) REFERENCES Teams(Id)
            )",
            @"CREATE TABLE IF NOT EXISTS Issues (
                Id TEXT PRIMARY KEY,
                Title TEXT NOT NULL,
                Description TEXT,
                Status INTEGER NOT NULL,
                Priority INTEGER NOT NULL,
                AssignedUserId TEXT,
                AssignedTeamId TEXT,
                CreatedDate TEXT NOT NULL,
                DueDate TEXT,
                ResolvedDate TEXT,
                LastModifiedDate TEXT NOT NULL
            )"
        };

        foreach (var sql in createTableStatements)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
    }
}
