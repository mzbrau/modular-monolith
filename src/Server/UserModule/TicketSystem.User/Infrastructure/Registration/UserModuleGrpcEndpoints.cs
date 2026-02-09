using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using TicketSystem.User.Infrastructure.Grpc;

namespace TicketSystem.User.Infrastructure.Registration;

public static class UserModuleGrpcEndpoints
{
    public static IEndpointRouteBuilder MapUserModuleGrpcServices(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGrpcService<UserGrpcService>();
        
        return endpoints;
    }
}
