var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.TicketSystem_Api>("ticketsystem-api");

builder.AddProject<Projects.TicketSystem_Client>("ticketsystem-client");

builder.Build().Run();
