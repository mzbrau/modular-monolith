using Microsoft.Extensions.DependencyInjection;
using TicketSystem.Team.Adapters;
using TicketSystem.Team.Contracts;
using TicketSystem.Team.Domain;
using TicketSystem.Team.Grpc;
using TicketSystem.Team.Infrastructure;

namespace TicketSystem.Team.Registration;

public static class TeamModuleRegistration
{
    public static IServiceCollection AddTeamModule(this IServiceCollection services)
    {
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
