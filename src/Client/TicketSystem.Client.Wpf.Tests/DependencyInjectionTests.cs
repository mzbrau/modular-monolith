using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketSystem.Client.Wpf.ViewModels;
using Xunit;
using GrpcIssueService = TicketSystem.Issue.Infrastructure.Grpc.IssueService;
using GrpcTeamService = TicketSystem.Team.Infrastructure.Grpc.TeamService;
using GrpcUserService = TicketSystem.User.Infrastructure.Grpc.UserService;
using WpfIssueService = TicketSystem.Client.Wpf.Services.IIssueService;
using WpfTeamService = TicketSystem.Client.Wpf.Services.ITeamService;
using WpfUserService = TicketSystem.Client.Wpf.Services.IUserService;
using WpfNavigationService = TicketSystem.Client.Wpf.Services.INavigationService;

namespace TicketSystem.Client.Wpf.Tests;

/// <summary>
/// Tests for dependency injection configuration.
/// Validates that all services, ViewModels, and gRPC clients are properly registered.
/// </summary>
/// <remarks>
/// Requirements:
/// - 1.6: THE WPF_Application SHALL support dependency injection for services and ViewModels
/// - 2.3: THE WPF_Application SHALL configure the gRPC server address (default: https://localhost:7001)
/// </remarks>
public class DependencyInjectionTests
{
    private IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        // Configuration
        var configurationData = new Dictionary<string, string?>
        {
            { "GrpcServer:Address", "https://localhost:7001" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Get gRPC server address from configuration
        var grpcAddress = configuration["GrpcServer:Address"] ?? "https://localhost:7001";

        // Configure gRPC clients
        services.AddSingleton(sp =>
        {
            var channel = GrpcChannel.ForAddress(grpcAddress);
            return new GrpcIssueService.IssueServiceClient(channel);
        });

        services.AddSingleton(sp =>
        {
            var channel = GrpcChannel.ForAddress(grpcAddress);
            return new GrpcTeamService.TeamServiceClient(channel);
        });

        services.AddSingleton(sp =>
        {
            var channel = GrpcChannel.ForAddress(grpcAddress);
            return new GrpcUserService.UserServiceClient(channel);
        });

        // Register services
        services.AddSingleton<WpfNavigationService, TicketSystem.Client.Wpf.Services.NavigationService>();
        services.AddTransient<WpfIssueService, TicketSystem.Client.Wpf.Services.IssueService>();
        services.AddTransient<WpfTeamService, TicketSystem.Client.Wpf.Services.TeamService>();
        services.AddTransient<WpfUserService, TicketSystem.Client.Wpf.Services.UserService>();

        // Register ViewModels
        services.AddTransient<IssuesViewModel>();
        services.AddTransient<TeamsViewModel>();
        services.AddTransient<UsersViewModel>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public void ServiceProvider_CanResolveConfiguration()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var configuration = serviceProvider.GetService<IConfiguration>();

        // Assert
        Assert.NotNull(configuration);
    }

    [Fact]
    public void ServiceProvider_ConfigurationHasCorrectGrpcAddress()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        // Act
        var grpcAddress = configuration["GrpcServer:Address"];

        // Assert
        Assert.Equal("https://localhost:7001", grpcAddress);
    }

    [Fact]
    public void ServiceProvider_CanResolveIssueServiceClient()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var client = serviceProvider.GetService<GrpcIssueService.IssueServiceClient>();

        // Assert
        Assert.NotNull(client);
    }

    [Fact]
    public void ServiceProvider_CanResolveTeamServiceClient()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var client = serviceProvider.GetService<GrpcTeamService.TeamServiceClient>();

        // Assert
        Assert.NotNull(client);
    }

    [Fact]
    public void ServiceProvider_CanResolveUserServiceClient()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var client = serviceProvider.GetService<GrpcUserService.UserServiceClient>();

        // Assert
        Assert.NotNull(client);
    }

    [Fact]
    public void ServiceProvider_CanResolveNavigationService()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var service = serviceProvider.GetService<WpfNavigationService>();

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TicketSystem.Client.Wpf.Services.NavigationService>(service);
    }

    [Fact]
    public void ServiceProvider_CanResolveIssueService()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var service = serviceProvider.GetService<WpfIssueService>();

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TicketSystem.Client.Wpf.Services.IssueService>(service);
    }

    [Fact]
    public void ServiceProvider_CanResolveTeamService()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var service = serviceProvider.GetService<WpfTeamService>();

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TicketSystem.Client.Wpf.Services.TeamService>(service);
    }

    [Fact]
    public void ServiceProvider_CanResolveUserService()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var service = serviceProvider.GetService<WpfUserService>();

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TicketSystem.Client.Wpf.Services.UserService>(service);
    }

    [Fact]
    public void ServiceProvider_CanResolveIssuesViewModel()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var viewModel = serviceProvider.GetService<IssuesViewModel>();

        // Assert
        Assert.NotNull(viewModel);
    }

    [Fact]
    public void ServiceProvider_CanResolveTeamsViewModel()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var viewModel = serviceProvider.GetService<TeamsViewModel>();

        // Assert
        Assert.NotNull(viewModel);
    }

    [Fact]
    public void ServiceProvider_CanResolveUsersViewModel()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var viewModel = serviceProvider.GetService<UsersViewModel>();

        // Assert
        Assert.NotNull(viewModel);
    }

    [Fact]
    public void ServiceProvider_NavigationServiceIsSingleton()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var service1 = serviceProvider.GetService<WpfNavigationService>();
        var service2 = serviceProvider.GetService<WpfNavigationService>();

        // Assert
        Assert.Same(service1, service2);
    }

    [Fact]
    public void ServiceProvider_IssueServiceIsTransient()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var service1 = serviceProvider.GetService<WpfIssueService>();
        var service2 = serviceProvider.GetService<WpfIssueService>();

        // Assert
        Assert.NotSame(service1, service2);
    }

    [Fact]
    public void ServiceProvider_ViewModelsAreTransient()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();

        // Act
        var viewModel1 = serviceProvider.GetService<IssuesViewModel>();
        var viewModel2 = serviceProvider.GetService<IssuesViewModel>();

        // Assert
        Assert.NotSame(viewModel1, viewModel2);
    }
}
