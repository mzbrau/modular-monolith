using Microsoft.Extensions.Configuration;
using Moq;
using TicketSystem.Client.Wpf.Services;
using Xunit;

namespace TicketSystem.Client.Wpf.Tests.Services;

/// <summary>
/// Unit tests for ConfigurationService.
/// </summary>
public class ConfigurationServiceTests
{
    /// <summary>
    /// Tests that GetServerAddress returns the configured address.
    /// </summary>
    [Fact]
    public void GetServerAddress_ReturnsConfiguredAddress()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["GrpcServer:Address"] = "https://test.example.com:8080"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
        var service = new ConfigurationService(configuration);

        // Act
        var address = service.GetServerAddress();

        // Assert
        Assert.Equal("https://test.example.com:8080", address);
    }

    /// <summary>
    /// Tests that GetServerAddress returns default address when not configured.
    /// </summary>
    [Fact]
    public void GetServerAddress_ReturnsDefaultWhenNotConfigured()
    {
        // Arrange
        var configData = new Dictionary<string, string?>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
        var service = new ConfigurationService(configuration);

        // Act
        var address = service.GetServerAddress();

        // Assert
        Assert.Equal("https://localhost:7001", address);
    }

    /// <summary>
    /// Tests that UpdateServerAddress updates the address.
    /// </summary>
    [Fact]
    public void UpdateServerAddress_UpdatesAddress()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["GrpcServer:Address"] = "https://localhost:7001"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
        var service = new ConfigurationService(configuration);
        var newAddress = "https://newserver.com:9000";

        // Act
        service.UpdateServerAddress(newAddress);
        var address = service.GetServerAddress();

        // Assert
        Assert.Equal(newAddress, address);
    }

    /// <summary>
    /// Tests that UpdateServerAddress raises ServerAddressChanged event.
    /// </summary>
    [Fact]
    public void UpdateServerAddress_RaisesServerAddressChangedEvent()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["GrpcServer:Address"] = "https://localhost:7001"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
        var service = new ConfigurationService(configuration);
        var newAddress = "https://newserver.com:9000";
        var eventRaised = false;
        var eventAddress = string.Empty;

        service.ServerAddressChanged += (sender, address) =>
        {
            eventRaised = true;
            eventAddress = address;
        };

        // Act
        service.UpdateServerAddress(newAddress);

        // Assert
        Assert.True(eventRaised);
        Assert.Equal(newAddress, eventAddress);
    }

    /// <summary>
    /// Tests that UpdateServerAddress throws when address is null or empty.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateServerAddress_ThrowsWhenAddressIsNullOrEmpty(string? address)
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["GrpcServer:Address"] = "https://localhost:7001"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
        var service = new ConfigurationService(configuration);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.UpdateServerAddress(address!));
    }
}
