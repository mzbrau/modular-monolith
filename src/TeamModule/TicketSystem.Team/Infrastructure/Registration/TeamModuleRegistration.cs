using Microsoft.Extensions.DependencyInjection;
using TicketSystem.Team.Application.Adapters;
using TicketSystem.Team.Contracts;
using TicketSystem.Team.Domain;
using TicketSystem.Team.Infrastructure.Grpc;
using TicketSystem.Team.Infrastructure;

namespace TicketSystem.Team.Infrastructure.Registration;

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
