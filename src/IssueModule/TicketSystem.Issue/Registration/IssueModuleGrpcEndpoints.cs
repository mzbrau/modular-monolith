using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using TicketSystem.Issue.Grpc;

namespace TicketSystem.Issue.Registration;

public static class IssueModuleGrpcEndpoints
{
    public static IEndpointRouteBuilder MapIssueModuleGrpcServices(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGrpcService<IssueGrpcService>();
        
        return endpoints;
    }
}
