using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text.Json;

namespace TicketSystem.Client.Wpf.Services;

/// <summary>
/// Service for managing application configuration.
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly string _configFilePath;
    private string _serverAddress;

    /// <summary>
    /// Event raised when the server address changes.
    /// </summary>
    public event EventHandler<string>? ServerAddressChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    public ConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        _serverAddress = _configuration["GrpcServer:Address"] ?? "https://localhost:7001";
    }

    /// <summary>
    /// Gets the current gRPC server address.
    /// </summary>
    public string GetServerAddress()
    {
        return _serverAddress;
    }

    /// <summary>
    /// Updates the gRPC server address.
    /// </summary>
    /// <param name="address">The new server address.</param>
    public void UpdateServerAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("Server address cannot be empty.", nameof(address));
        }

        _serverAddress = address;
        ServerAddressChanged?.Invoke(this, address);
    }

    /// <summary>
    /// Saves the configuration to persistent storage.
    /// </summary>
    public void SaveConfiguration()
    {
        try
        {
            // Read the existing configuration file
            var json = File.ReadAllText(_configFilePath);
            var config = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            if (config == null)
            {
                config = new Dictionary<string, object>();
            }

            // Update the GrpcServer section
            if (!config.ContainsKey("GrpcServer"))
            {
                config["GrpcServer"] = new Dictionary<string, object>();
            }

            var grpcServer = config["GrpcServer"] as JsonElement?;
            if (grpcServer.HasValue)
            {
                var grpcServerDict = JsonSerializer.Deserialize<Dictionary<string, object>>(grpcServer.Value.GetRawText());
                if (grpcServerDict != null)
                {
                    grpcServerDict["Address"] = _serverAddress;
                    config["GrpcServer"] = grpcServerDict;
                }
            }
            else
            {
                config["GrpcServer"] = new Dictionary<string, object>
                {
                    ["Address"] = _serverAddress
                };
            }

            // Write the updated configuration back to the file
            var options = new JsonSerializerOptions { WriteIndented = true };
            var updatedJson = JsonSerializer.Serialize(config, options);
            File.WriteAllText(_configFilePath, updatedJson);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to save configuration.", ex);
        }
    }
}
