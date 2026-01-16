namespace TicketSystem.Api.Database;

public sealed class DatabaseOptions
{
    public DatabaseOptions(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; }
}
