namespace TicketSystem.Client.Wpf.Services;

/// <summary>
/// Service for managing application configuration.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Gets the current gRPC server address.
    /// </summary>
    string GetServerAddress();

    /// <summary>
    /// Updates the gRPC server address.
    /// </summary>
    /// <param name="address">The new server address.</param>
    void UpdateServerAddress(string address);

    /// <summary>
    /// Saves the configuration to persistent storage.
    /// </summary>
    void SaveConfiguration();

    /// <summary>
    /// Event raised when the server address changes.
    /// </summary>
    event EventHandler<string>? ServerAddressChanged;
}
