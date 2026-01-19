using Fig.Aspire;
using System.Net;
using System.Net.NetworkInformation;

var builder = DistributedApplication.CreateBuilder(args);

// Add Fig API container
var figApi = builder.AddFigApi("fig-api", 7281)
    .WithEnvironment("ApiSettings:DbConnectionString=Server", $"{GetFqdn()};User Id=fig_login;Password=495472f157684ab1a38fGGGPPP6f54e4aba64e;Initial Catalog=fig;TrustServerCertificate=True");

// Add Fig Web container and pass a reference to the API
var figWeb = builder.AddFigWeb("fig-web", 7148)
    .WithFigApiReference(figApi);


// Add the API project with reference to Fig
var api = builder.AddProject<Projects.TicketSystem_Api>("ticketsystem-api")
    .WithReference(figApi)  // Ensures that the API knows where to contact Fig
    .WaitFor(figApi) // API will wait for Fig to start
    .WithArgs("--secret=a984efe5b49b40ffaf53428cec9530b7");       

// Add the client project with reference to the API
builder.AddProject<Projects.TicketSystem_Client>("ticketsystem-client")
    .WithReference(api) // Client knows where to contact the API via service discovery
    .WaitFor(api);

builder.Build().Run();



static string GetFqdn()
{
    var domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
    var hostName = Dns.GetHostName();

    domainName = "." + domainName;
    if (!hostName.EndsWith(domainName))
    {
        hostName += domainName;
    }

    return hostName;
}