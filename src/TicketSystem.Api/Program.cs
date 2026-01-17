using NHibernate;
using TicketSystem.Api.Database;
using TicketSystem.Issue.Registration;
using TicketSystem.Team.Registration;
using TicketSystem.User.Registration;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure NHibernate
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "TicketSystem.db");
var connectionString = $"Data Source={dbPath};Version=3;";
var sessionFactory = NHibernateConfiguration.CreateSessionFactory(connectionString);

builder.Services.AddSingleton(sessionFactory);
builder.Services.AddSingleton(new DatabaseOptions(connectionString));
builder.Services.AddScoped(sp => sp.GetRequiredService<ISessionFactory>().OpenSession());
builder.Services.AddSingleton<DatabaseInitializer>();

// Add module registrations
builder.Services.AddUserModule();
builder.Services.AddTeamModule();
builder.Services.AddIssueModule();

// Add gRPC services from modules
builder.Services.AddUserModuleGrpcServices();
builder.Services.AddTeamModuleGrpcServices();
builder.Services.AddIssueModuleGrpcServices();

var app = builder.Build();

app.MapDefaultEndpoints();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    dbInitializer.Initialize();
}

// Configure middleware for transaction handling
app.UseNHibernateTransaction();

// Map gRPC services from modules
app.MapUserModuleGrpcServices();
app.MapTeamModuleGrpcServices();
app.MapIssueModuleGrpcServices();

app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();