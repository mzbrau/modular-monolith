using Microsoft.Extensions.DependencyInjection;
using TicketSystem.Team.Adapters;
using TicketSystem.Team.Application;
using TicketSystem.Team.Contracts;
using TicketSystem.Team.Domain;
using TicketSystem.Team.Infrastructure;

namespace TicketSystem.Team.Registration;

public static class TeamModuleRegistration
{
    public static IServiceCollection AddTeamModule(this IServiceCollection services)
    {
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<TeamService>();
        services.AddScoped<ITeamModuleApi, TeamModuleApiAdapter>();
        
        return services;
    }
}
