using Fig.Client.ExtensionMethods;
using Fig.Client.SecretProvider.Docker;
using Fig.Client.SecretProvider.Dpapi;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using NHibernate;
using TicketSystem.Api.Configuration;
using TicketSystem.Api.Database;
using TicketSystem.Issue.Infrastructure.Registration;
using TicketSystem.Team.Infrastructure.Registration;
using TicketSystem.User.Infrastructure.Registration;

var builder = WebApplication.CreateBuilder(args);

var logger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("Startup");
logger.LogInformation("Starting TicketSystem API application");

// Add Fig as a configuration provider before other services
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

builder.Configuration.AddFig<TicketSystemSettings>(options =>
{
    options.ClientName = "TicketSystemApi";
    options.LoggerFactory = loggerFactory;
    options.CommandLineArgs = args;
    options.ClientSecretProviders = [new DockerSecretProvider(), new DpapiSecretProvider()];
});

builder.AddServiceDefaults();

// Configure NHibernate
// Check if DatabaseOptions is already registered (e.g., by test configuration)
var existingDbOptions = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(DatabaseOptions));
var connectionString = existingDbOptions != null 
    ? ((DatabaseOptions)existingDbOptions.ImplementationInstance!).ConnectionString
    : $"Data Source={Path.Combine(builder.Environment.ContentRootPath, "TicketSystem.db")};Version=3;";

logger.LogInformation("Database connection string: {ConnectionString}", connectionString);

var sessionFactory = NHibernateConfiguration.CreateSessionFactory(connectionString, [
    typeof(UserModuleRegistration).Assembly,
    typeof(TeamModuleRegistration).Assembly,
    typeof(IssueModuleRegistration).Assembly
]);

builder.Services.AddSingleton(sessionFactory);
// Only add DatabaseOptions if not already registered
if (existingDbOptions == null)
{
    builder.Services.AddSingleton(new DatabaseOptions(connectionString));
}
builder.Services.AddScoped(sp => sp.GetRequiredService<ISessionFactory>().OpenSession());
builder.Services.AddSingleton<DatabaseInitializer>();

// Register Fig and configure settings
builder.Host.UseFig<TicketSystemSettings>();
builder.Services.Configure<TicketSystemSettings>(builder.Configuration);

// Add module registrations with configuration
logger.LogInformation("Registering modules: User, Team, Issue");
builder.Services.AddUserModule(builder.Configuration);
builder.Services.AddTeamModule(builder.Configuration);
builder.Services.AddIssueModule(builder.Configuration);

// Add gRPC services from modules
// Add health checks
builder.Services.AddHealthChecks();

builder.Services.AddUserModuleGrpcServices();
builder.Services.AddTeamModuleGrpcServices();
builder.Services.AddIssueModuleGrpcServices();

var app = builder.Build();

app.MapDefaultEndpoints();

var appLogger = app.Services.GetRequiredService<ILogger<Program>>();
appLogger.LogInformation("Initializing database schema");

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    dbInitializer.Initialize();
}

//Add health check endpoint
app.MapHealthChecks("/_health", new HealthCheckOptions()
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// 
appLogger.LogInformation("Configuring middleware pipeline");

// Configure middleware for transaction handling
app.UseNHibernateTransaction();

// Map gRPC services from modules
appLogger.LogInformation("Mapping gRPC services for all modules");
app.MapUserModuleGrpcServices();
app.MapTeamModuleGrpcServices();
app.MapIssueModuleGrpcServices();

app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

appLogger.LogInformation("TicketSystem API starting on {Urls}", string.Join(", ", builder.Configuration["ASPNETCORE_URLS"]?.Split(';') ??
    ["default"]));
app.Run();