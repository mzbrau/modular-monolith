namespace TicketSystem.Client.Wpf.Services;

/// <summary>
/// Service for displaying dialogs to the user.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a confirmation dialog with Yes/No buttons.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="title">The dialog title.</param>
    /// <returns>True if the user clicked Yes, false otherwise.</returns>
    bool ShowConfirmation(string message, string title);

    /// <summary>
    /// Shows an error dialog with an OK button.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    /// <param name="title">The dialog title.</param>
    void ShowError(string message, string title);

    /// <summary>
    /// Shows an information dialog with an OK button.
    /// </summary>
    /// <param name="message">The information message to display.</param>
    /// <param name="title">The dialog title.</param>
    void ShowInformation(string message, string title);
}
