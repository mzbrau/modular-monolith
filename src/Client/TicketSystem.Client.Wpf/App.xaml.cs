using System.Windows;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketSystem.Client.Wpf.Services;
using TicketSystem.Client.Wpf.ViewModels;
using TicketSystem.Issue.Infrastructure.Grpc;
using TicketSystem.Team.Infrastructure.Grpc;
using TicketSystem.User.Infrastructure.Grpc;

namespace TicketSystem.Client.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// Configures dependency injection and application startup.
/// </summary>
/// <remarks>
/// Requirements:
/// - 1.6: THE WPF_Application SHALL support dependency injection for services and ViewModels
/// - 2.1: WHEN the WPF_Application starts, THE WPF_Application SHALL establish gRPC channel connections to the backend services
/// - 2.3: THE WPF_Application SHALL configure the gRPC server address (default: https://localhost:7001)
/// - 17.1: THE WPF_Application SHALL read the gRPC server address from a configuration file
/// - 17.2: THE WPF_Application SHALL use https://localhost:7001 as the default gRPC server address
/// </remarks>
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Configures all services, gRPC clients, ViewModels, and the main window in the DI container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    private void ConfigureServices(IServiceCollection services)
    {
        // Configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Get gRPC server address from configuration
        var grpcAddress = configuration["GrpcServer:Address"] ?? "https://localhost:7001";

        // Configure gRPC clients
        services.AddSingleton(sp =>
        {
            var channel = GrpcChannel.ForAddress(grpcAddress);
            return new TicketSystem.Issue.Infrastructure.Grpc.IssueService.IssueServiceClient(channel);
        });

        services.AddSingleton(sp =>
        {
            var channel = GrpcChannel.ForAddress(grpcAddress);
            return new TicketSystem.Team.Infrastructure.Grpc.TeamService.TeamServiceClient(channel);
        });

        services.AddSingleton(sp =>
        {
            var channel = GrpcChannel.ForAddress(grpcAddress);
            return new TicketSystem.User.Infrastructure.Grpc.UserService.UserServiceClient(channel);
        });

        // Register services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddTransient<IIssueService, Services.IssueService>();
        services.AddTransient<ITeamService, Services.TeamService>();
        services.AddTransient<IUserService, Services.UserService>();

        // Register ViewModels
        services.AddTransient<IssuesViewModel>();
        services.AddTransient<TeamsViewModel>();
        services.AddTransient<UsersViewModel>();

        // Register Main Window
        services.AddSingleton<MainWindow>();
    }

    /// <summary>
    /// Handles application startup by resolving and showing the main window.
    /// </summary>
    /// <param name="e">Startup event arguments.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Resolve and show the main window
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
