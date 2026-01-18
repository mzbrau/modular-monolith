using Fig.Client.ExtensionMethods;
using Fig.Client.Testing.Extensions;
using Fig.Client.Testing.Integration;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
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
        
        // Register test database path
        var connectionString = $"Data Source={_testDatabasePath};Version=3;";
        services.AddSingleton(new DatabaseOptions(connectionString));
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
