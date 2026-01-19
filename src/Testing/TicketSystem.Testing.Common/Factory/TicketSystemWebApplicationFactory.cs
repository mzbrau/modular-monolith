using Fig.Client.ExtensionMethods;
using Fig.Client.Testing.Extensions;
using Fig.Client.Testing.Integration;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NHibernate;
using TicketSystem.Api.Configuration;
using TicketSystem.Api.Database;

namespace TicketSystem.Testing.Common.Factory;

public class TicketSystemWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly TicketSystemSettings _settings;
    private readonly string _testDatabasePath;
    private ConfigReloader<TicketSystemSettings> ConfigReloader { get; } = new();
    
    protected TicketSystemSettings Settings => _settings;
    
    public TicketSystemWebApplicationFactory()
    {
        _settings = new ();
        _testDatabasePath = Path.Combine(
            Path.GetTempPath(), 
            $"TicketSystem_Test_{Guid.NewGuid():N}.db");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Configure test-specific settings
        builder.DisableFig();
        builder.ConfigureAppConfiguration((context, conf) =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddIntegrationTestConfiguration(ConfigReloader, Settings); // Adds a json based configuration provider that
                // allows settings to be injected and
                // updated during test execution.
                config.Build();
            });
        });
        
        // 3. Override services for testing
        builder.ConfigureServices((context, services) =>
        {
            // Override database connection to use test database
            ConfigureTestDatabase(services);
            
            // Add any additional test-specific service overrides
            ConfigureTestServices(services);
        });
        
        builder.UseEnvironment("Testing");
    }

    private void ConfigureTestDatabase(IServiceCollection services)
    {
        // Remove existing DatabaseOptions registration
        var descriptor = services.SingleOrDefault(d => 
            d.ServiceType == typeof(DatabaseOptions));
        if (descriptor != null)
            services.Remove(descriptor);
        
        // Remove existing SessionFactory registration as it was created with the wrong connection string
        var sessionFactoryDescriptor = services.SingleOrDefault(d =>
            d.ServiceType == typeof(ISessionFactory));
        if (sessionFactoryDescriptor != null)
            services.Remove(sessionFactoryDescriptor);
        
        // Register test database path
        var connectionString = $"Data Source={_testDatabasePath};Version=3;";
        services.AddSingleton(new DatabaseOptions(connectionString));
        
        // Re-create session factory with test database connection string
        // Find the module registration assemblies dynamically to avoid architecture violations
        var moduleAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName?.Contains("TicketSystem.") == true &&
                       (a.FullName.Contains(".User,") || a.FullName.Contains(".Team,") || a.FullName.Contains(".Issue,")))
            .ToArray();
        
        var sessionFactory = NHibernateConfiguration.CreateSessionFactory(connectionString, moduleAssemblies);
        services.AddSingleton(sessionFactory);
    }

    private void ConfigureTestServices(IServiceCollection services)
    {
        // Hook point for additional test service configuration
        // Can be extended via virtual method or callbacks
    }

    /// <summary>
    /// Updates configuration values during test execution.
    /// Note: This creates a new factory instance. For dynamic updates, 
    /// you'd need to restart the application or use a more sophisticated approach.
    /// </summary>
    public void UpdateSettings()
    {
        ConfigReloader.Reload(_settings);
    }

    /// <summary>
    /// Gets a scoped service from the test server's DI container.
    /// </summary>
    public T GetRequiredService<T>() where T : notnull
    {
        return Services.CreateScope().ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Ensures the database schema is initialized.
    /// This should be called after creating the factory to ensure all tables exist.
    /// </summary>
    public void EnsureDatabaseInitialized()
    {
        using var scope = Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        
        // Try to get DatabaseInitializer from DI, or create one if needed
        DatabaseInitializer? dbInitializer = null;
        try
        {
            dbInitializer = serviceProvider.GetService<DatabaseInitializer>();
        }
        catch
        {
            // DatabaseInitializer might not be registered yet
        }
        
        if (dbInitializer == null)
        {
            // Create DatabaseInitializer manually if not registered
            var databaseOptions = serviceProvider.GetRequiredService<DatabaseOptions>();
            var logger = serviceProvider.GetRequiredService<ILogger<DatabaseInitializer>>();
            dbInitializer = new DatabaseInitializer(databaseOptions, logger);
        }
        
        dbInitializer.Initialize();
    }

    /// <summary>
    /// Creates a gRPC channel for testing gRPC services.
    /// </summary>
    public GrpcChannel CreateGrpcChannel()
    {
        var client = CreateClient();
        return GrpcChannel.ForAddress(client.BaseAddress!, new GrpcChannelOptions
        {
            HttpHandler = Server.CreateHandler()
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        
        // Cleanup test database
        if (File.Exists(_testDatabasePath))
        {
            try { File.Delete(_testDatabasePath); }
            catch { /* Best effort cleanup */ }
        }
    }
}
