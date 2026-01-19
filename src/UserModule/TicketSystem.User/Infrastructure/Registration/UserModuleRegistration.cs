using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketSystem.User.Application.Adapters;
using TicketSystem.User.Configuration;
using TicketSystem.User.Contracts;
using TicketSystem.User.Domain;
using TicketSystem.User.Infrastructure.Grpc;

namespace TicketSystem.User.Infrastructure.Registration;

public static class UserModuleRegistration
{
    public static IServiceCollection AddUserModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure UserSettings from the parent settings
        services.Configure<UserSettings>(configuration.GetSection("User"));
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<Application.UserService>();
        services.AddScoped<IUserModuleApi, UserModuleApiAdapter>();
        
        return services;
    }

    public static IServiceCollection AddUserModuleGrpcServices(this IServiceCollection services)
    {
        services.AddScoped<UserGrpcService>();
        
        return services;
    }
}
