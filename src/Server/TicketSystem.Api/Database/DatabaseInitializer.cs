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
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Email TEXT NOT NULL UNIQUE,
                FirstName TEXT NOT NULL,
                LastName TEXT NOT NULL,
                CreatedDate TEXT NOT NULL,
                IsActive INTEGER NOT NULL DEFAULT 1
            )",
            @"CREATE TABLE IF NOT EXISTS Teams (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT,
                CreatedDate TEXT NOT NULL
            )",
            @"CREATE TABLE IF NOT EXISTS TeamMembers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TeamId INTEGER NOT NULL,
                UserId INTEGER NOT NULL,
                JoinedDate TEXT NOT NULL,
                Role INTEGER NOT NULL,
                FOREIGN KEY (TeamId) REFERENCES Teams(Id)
            )",
            @"CREATE TABLE IF NOT EXISTS Issues (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Description TEXT,
                Status INTEGER NOT NULL,
                Priority INTEGER NOT NULL,
                AssignedUserId INTEGER,
                AssignedTeamId INTEGER,
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
