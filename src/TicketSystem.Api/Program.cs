using NHibernate;
using TicketSystem.Api.Database;
using TicketSystem.Issue.Infrastructure.Registration;
using TicketSystem.Team.Infrastructure.Registration;
using TicketSystem.User.Infrastructure.Registration;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var logger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("Startup");
logger.LogInformation("Starting TicketSystem API application");

// Configure NHibernate
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "TicketSystem.db");
var connectionString = $"Data Source={dbPath};Version=3;";
logger.LogInformation("Database path: {DatabasePath}", dbPath);

var sessionFactory = NHibernateConfiguration.CreateSessionFactory(connectionString);

builder.Services.AddSingleton(sessionFactory);
builder.Services.AddSingleton(new DatabaseOptions(connectionString));
builder.Services.AddScoped(sp => sp.GetRequiredService<ISessionFactory>().OpenSession());
builder.Services.AddSingleton<DatabaseInitializer>();

// Add module registrations
logger.LogInformation("Registering modules: User, Team, Issue");
builder.Services.AddUserModule();
builder.Services.AddTeamModule();
builder.Services.AddIssueModule();

// Add gRPC services from modules
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

appLogger.LogInformation("TicketSystem API starting on {Urls}", string.Join(", ", builder.Configuration["ASPNETCORE_URLS"]?.Split(';') ?? new[] { "default" }));
app.Run();