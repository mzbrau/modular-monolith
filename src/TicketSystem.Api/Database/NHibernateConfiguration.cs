using NHibernate;
using NHibernate.Dialect;
using NHibernate.Driver;

namespace TicketSystem.Api.Database;

public static class NHibernateConfiguration
{
    public static ISessionFactory CreateSessionFactory(string connectionString, IEnumerable<System.Reflection.Assembly> assembliesToScan)
    {
        var configuration = new NHibernate.Cfg.Configuration();
        
        configuration.DataBaseIntegration(db =>
        {
            db.ConnectionString = connectionString;
            db.Dialect<SQLiteDialect>();
            db.Driver<SQLite20Driver>();
            db.LogSqlInConsole = true;
            db.LogFormattedSql = true;
        });

        foreach (var assembly in assembliesToScan)
        {
            configuration.AddAssembly(assembly);
        }

        return configuration.BuildSessionFactory();
    }
}
