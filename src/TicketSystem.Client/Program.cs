using TicketSystem.Client.Components;
using TicketSystem.Client.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure gRPC clients
var grpcAddress = builder.Configuration.GetValue<string>("GrpcAddress") ?? "https://localhost:5001";
builder.Services.AddSingleton(new UserGrpcClient(grpcAddress));
builder.Services.AddSingleton(new TeamGrpcClient(grpcAddress));
builder.Services.AddSingleton(new IssueGrpcClient(grpcAddress));

var app = builder.Build();

app.MapDefaultEndpoints();

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