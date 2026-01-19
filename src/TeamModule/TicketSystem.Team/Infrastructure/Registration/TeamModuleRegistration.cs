using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketSystem.Team.Application.Adapters;
using TicketSystem.Team.Application.Configuration;
using TicketSystem.Team.Contracts;
using TicketSystem.Team.Domain;
using TicketSystem.Team.Infrastructure.Grpc;

namespace TicketSystem.Team.Infrastructure.Registration;

public static class TeamModuleRegistration
{
    public static IServiceCollection AddTeamModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure TeamSettings from the parent settings
        services.Configure<TeamSettings>(configuration.GetSection("Team"));
        
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<Application.TeamService>();
        services.AddScoped<ITeamModuleApi, TeamModuleApiAdapter>();
        
        return services;
    }

    public static IServiceCollection AddTeamModuleGrpcServices(this IServiceCollection services)
    {
        services.AddGrpc();
        services.AddScoped<TeamGrpcService>();
        
        return services;
    }
}
