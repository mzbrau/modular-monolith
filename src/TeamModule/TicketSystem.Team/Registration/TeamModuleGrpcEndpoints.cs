using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using TicketSystem.Team.Grpc;

namespace TicketSystem.Team.Registration;

public static class TeamModuleGrpcEndpoints
{
    public static IEndpointRouteBuilder MapTeamModuleGrpcServices(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGrpcService<TeamGrpcService>();
        
        return endpoints;
    }
}
