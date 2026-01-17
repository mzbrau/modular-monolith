using Microsoft.Extensions.DependencyInjection;
using TicketSystem.Issue.Adapters;
using TicketSystem.Issue.Contracts;
using TicketSystem.Issue.Domain;
using TicketSystem.Issue.Grpc;
using TicketSystem.Issue.Infrastructure;

namespace TicketSystem.Issue.Registration;

public static class IssueModuleRegistration
{
    public static IServiceCollection AddIssueModule(this IServiceCollection services)
    {
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
