using System.Windows;
using TicketSystem.Client.Wpf.ViewModels;

namespace TicketSystem.Client.Wpf.Views;

/// <summary>
/// Interaction logic for SettingsDialog.xaml
/// </summary>
public partial class SettingsDialog : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsDialog"/> class.
    /// </summary>
    public SettingsDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsDialog"/> class with a ViewModel.
    /// </summary>
    /// <param name="viewModel">The settings view model.</param>
    public SettingsDialog(SettingsViewModel viewModel) : this()
    {
        DataContext = viewModel;
        
        // Subscribe to events
        viewModel.CloseRequested += (s, e) => Close();
    }
}
