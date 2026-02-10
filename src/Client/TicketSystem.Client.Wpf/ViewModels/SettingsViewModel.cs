using System.Windows.Input;
using TicketSystem.Client.Wpf.Commands;

namespace TicketSystem.Client.Wpf.ViewModels;

/// <summary>
/// ViewModel for application settings.
/// </summary>
public class SettingsViewModel : BaseViewModel
{
    private string _serverAddress = string.Empty;
    private string? _validationError;

    /// <summary>
    /// Gets or sets the gRPC server address.
    /// </summary>
    public string ServerAddress
    {
        get => _serverAddress;
        set => SetProperty(ref _serverAddress, value);
    }

    /// <summary>
    /// Gets or sets the validation error message.
    /// </summary>
    public string? ValidationError
    {
        get => _validationError;
        set => SetProperty(ref _validationError, value);
    }

    /// <summary>
    /// Gets the command to save settings.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Gets the command to cancel settings changes.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Event raised when settings should be saved.
    /// </summary>
    public event EventHandler? SettingsSaved;

    /// <summary>
    /// Event raised when settings dialog should be closed.
    /// </summary>
    public event EventHandler? CloseRequested;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
    /// </summary>
    public SettingsViewModel()
    {
        SaveCommand = new RelayCommand(_ => SaveSettings(), _ => CanSave());
        CancelCommand = new RelayCommand(_ => Cancel());
    }

    /// <summary>
    /// Validates the server address.
    /// </summary>
    private bool ValidateServerAddress()
    {
        ValidationError = null;

        if (string.IsNullOrWhiteSpace(ServerAddress))
        {
            ValidationError = "Server address cannot be empty.";
            return false;
        }

        if (!Uri.TryCreate(ServerAddress, UriKind.Absolute, out var uri))
        {
            ValidationError = "Server address must be a valid URL.";
            return false;
        }

        if (uri.Scheme != "https" && uri.Scheme != "http")
        {
            ValidationError = "Server address must use HTTP or HTTPS protocol.";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether settings can be saved.
    /// </summary>
    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(ServerAddress);
    }

    /// <summary>
    /// Saves the settings.
    /// </summary>
    private void SaveSettings()
    {
        if (!ValidateServerAddress())
        {
            return;
        }

        SettingsSaved?.Invoke(this, EventArgs.Empty);
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Cancels the settings changes.
    /// </summary>
    private void Cancel()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
