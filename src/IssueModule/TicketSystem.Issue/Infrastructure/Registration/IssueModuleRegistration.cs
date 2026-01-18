using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketSystem.Issue.Application.Adapters;
using TicketSystem.Issue.Configuration;
using TicketSystem.Issue.Contracts;
using TicketSystem.Issue.Domain;
using TicketSystem.Issue.Infrastructure.Grpc;

namespace TicketSystem.Issue.Infrastructure.Registration;

public static class IssueModuleRegistration
{
    public static IServiceCollection AddIssueModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure IssueSettings from the parent settings
        services.Configure<IssueSettings>(configuration.GetSection("Issue"));
        
        services.AddScoped<IIssueRepository, IssueRepository>();
        services.AddScoped<Application.IssueService>();
        services.AddScoped<IIssueModuleApi, IssueModuleApiAdapter>();
        
        return services;
    }

    public static IServiceCollection AddIssueModuleGrpcServices(this IServiceCollection services)
    {
        services.AddGrpc();
        services.AddScoped<IssueGrpcService>();
        
        return services;
    }
}
