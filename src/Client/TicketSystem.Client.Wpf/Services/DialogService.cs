using System.Windows;

namespace TicketSystem.Client.Wpf.Services;

/// <summary>
/// Service for displaying dialogs to the user using WPF MessageBox.
/// </summary>
public class DialogService : IDialogService
{
    /// <summary>
    /// Shows a confirmation dialog with Yes/No buttons.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="title">The dialog title.</param>
    /// <returns>True if the user clicked Yes, false otherwise.</returns>
    public bool ShowConfirmation(string message, string title)
    {
        var result = MessageBox.Show(
            message,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        return result == MessageBoxResult.Yes;
    }

    /// <summary>
    /// Shows an error dialog with an OK button.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    /// <param name="title">The dialog title.</param>
    public void ShowError(string message, string title)
    {
        MessageBox.Show(
            message,
            title,
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    /// <summary>
    /// Shows an information dialog with an OK button.
    /// </summary>
    /// <param name="message">The information message to display.</param>
    /// <param name="title">The dialog title.</param>
    public void ShowInformation(string message, string title)
    {
        MessageBox.Show(
            message,
            title,
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
