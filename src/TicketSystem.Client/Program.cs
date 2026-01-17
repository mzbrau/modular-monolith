using TicketSystem.Client.Components;
using TicketSystem.Client.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure gRPC clients
var grpcAddress = builder.Configuration.GetValue<string>("GrpcAddress") ?? "https://localhost:5001";

builder.Services.AddSingleton<UserGrpcClient>(sp => 
{
    var logger = sp.GetRequiredService<ILogger<UserGrpcClient>>();
    return new UserGrpcClient(grpcAddress, logger);
});

builder.Services.AddSingleton<TeamGrpcClient>(sp => 
{
    var logger = sp.GetRequiredService<ILogger<TeamGrpcClient>>();
    return new TeamGrpcClient(grpcAddress, logger);
});

builder.Services.AddSingleton<IssueGrpcClient>(sp => 
{
    var logger = sp.GetRequiredService<ILogger<IssueGrpcClient>>();
    return new IssueGrpcClient(grpcAddress, logger);
});

var app = builder.Build();

app.MapDefaultEndpoints();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("TicketSystem Client starting with gRPC address: {GrpcAddress}", grpcAddress);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();