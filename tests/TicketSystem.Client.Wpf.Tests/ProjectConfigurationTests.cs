using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace TicketSystem.Client.Wpf.Tests;

/// <summary>
/// Tests for project configuration to verify requirements 1.2, 1.5, 2.3, 17.2
/// </summary>
public class ProjectConfigurationTests
{
    [Fact]
    public void Project_Should_Target_Net10_Windows()
    {
        // Arrange & Act
        var assembly = typeof(App).Assembly;
        var targetFrameworkAttribute = assembly.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>();

        // Assert
        targetFrameworkAttribute.Should().NotBeNull("the assembly should have a target framework attribute");
        targetFrameworkAttribute!.FrameworkName.Should().Contain(".NETCoreApp,Version=v10.0", 
            "the project should target .NET 10");
    }

    [Fact]
    public void Generated_GrpcClients_Should_Be_Available()
    {
        // Arrange & Act
        var assembly = typeof(App).Assembly;
        var types = assembly.GetTypes();

        // Assert - Check for generated gRPC client types
        types.Should().Contain(t => t.Name == "IssueServiceClient", 
            "IssueService gRPC client should be generated from proto file");
        types.Should().Contain(t => t.Name == "TeamServiceClient", 
            "TeamService gRPC client should be generated from proto file");
        types.Should().Contain(t => t.Name == "UserServiceClient", 
            "UserService gRPC client should be generated from proto file");
    }

    [Fact]
    public void AppSettings_Should_Have_Default_GrpcServer_Address()
    {
        // Arrange
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // Act
        var grpcAddress = configuration["GrpcServer:Address"];

        // Assert
        grpcAddress.Should().NotBeNullOrEmpty("appsettings.json should contain GrpcServer:Address");
        grpcAddress.Should().Be("https://localhost:7001", 
            "the default gRPC server address should be https://localhost:7001");
    }

    [Fact]
    public void AppSettings_File_Should_Exist_In_Output_Directory()
    {
        // Arrange
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var appSettingsPath = Path.Combine(basePath, "appsettings.json");

        // Act & Assert
        File.Exists(appSettingsPath).Should().BeTrue(
            "appsettings.json should be copied to the output directory");
    }

    [Fact]
    public void Configuration_Should_Be_Loadable()
    {
        // Arrange & Act
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        Action act = () =>
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        };

        // Assert
        act.Should().NotThrow("configuration should be loadable without errors");
    }
}
