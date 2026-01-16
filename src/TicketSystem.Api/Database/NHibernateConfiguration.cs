using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping;

namespace TicketSystem.Api.Database;

public static class NHibernateConfiguration
{
    public static ISessionFactory CreateSessionFactory(string connectionString)
    {
        var configuration = new Configuration();
        
        configuration.DataBaseIntegration(db =>
        {
            db.ConnectionString = connectionString;
            db.Dialect<SQLiteDialect>();
            db.Driver<SQLite20Driver>();
            db.LogSqlInConsole = true;
            db.LogFormattedSql = true;
        });

        // Load all embedded mapping files from the API assembly
        var assembly = typeof(NHibernateConfiguration).Assembly;
        var resourceNames = assembly.GetManifestResourceNames();
        
        foreach (var resourceName in resourceNames.Where(r => r.EndsWith(".hbm.xml")))
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                configuration.AddInputStream(stream);
            }
        }

        return configuration.BuildSessionFactory();
    }
}
