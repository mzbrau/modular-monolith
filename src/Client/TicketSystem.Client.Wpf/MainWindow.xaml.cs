using System.ComponentModel;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TicketSystem.Client.Wpf.Services;
using TicketSystem.Client.Wpf.ViewModels;
using TicketSystem.Client.Wpf.Views;

namespace TicketSystem.Client.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// Manages navigation between different views using the NavigationService.
/// </summary>
/// <remarks>
/// Requirements:
/// - 16.2: WHEN a user clicks a navigation control, THE WPF_Application SHALL display the corresponding view
/// - 16.3: THE WPF_Application SHALL maintain navigation state so users can return to previous views
/// </remarks>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly INavigationService _navigationService;
    private readonly IConfigurationService _configurationService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IssuesViewModel _issuesViewModel;
    private readonly TeamsViewModel _teamsViewModel;
    private readonly UsersViewModel _usersViewModel;
    private BaseViewModel? _currentViewModel;

    /// <summary>
    /// Initializes a new instance of the MainWindow class.
    /// </summary>
    /// <param name="navigationService">The navigation service for managing view navigation.</param>
    /// <param name="configurationService">The configuration service for managing settings.</param>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    /// <param name="issuesViewModel">The ViewModel for the Issues view.</param>
    /// <param name="teamsViewModel">The ViewModel for the Teams view.</param>
    /// <param name="usersViewModel">The ViewModel for the Users view.</param>
    public MainWindow(
        INavigationService navigationService,
        IConfigurationService configurationService,
        IServiceProvider serviceProvider,
        IssuesViewModel issuesViewModel,
        TeamsViewModel teamsViewModel,
        UsersViewModel usersViewModel)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _issuesViewModel = issuesViewModel ?? throw new ArgumentNullException(nameof(issuesViewModel));
        _teamsViewModel = teamsViewModel ?? throw new ArgumentNullException(nameof(teamsViewModel));
        _usersViewModel = usersViewModel ?? throw new ArgumentNullException(nameof(usersViewModel));

        InitializeComponent();

        // Set the DataContext to this window for binding CurrentViewModel
        DataContext = this;

        // Subscribe to navigation service events
        _navigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;

        // Navigate to Issues view by default
        NavigateToIssues();
    }

    /// <summary>
    /// Gets the current ViewModel being displayed.
    /// </summary>
    public BaseViewModel? CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            if (_currentViewModel != value)
            {
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
                UpdateNavigationButtonStates();
            }
        }
    }

    /// <summary>
    /// Event raised when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Handles the Issues button click event.
    /// </summary>
    private void IssuesButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToIssues();
    }

    /// <summary>
    /// Handles the Teams button click event.
    /// </summary>
    private void TeamsButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToTeams();
    }

    /// <summary>
    /// Handles the Users button click event.
    /// </summary>
    private void UsersButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToUsers();
    }

    /// <summary>
    /// Handles the Settings menu item click event.
    /// </summary>
    private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var settingsViewModel = new SettingsViewModel
        {
            ServerAddress = _configurationService.GetServerAddress()
        };

        settingsViewModel.SettingsSaved += (s, args) =>
        {
            try
            {
                _configurationService.UpdateServerAddress(settingsViewModel.ServerAddress);
                _configurationService.SaveConfiguration();
                StatusText.Text = "Settings saved successfully. Restart the application for changes to take effect.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save settings: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        };

        var settingsDialog = new SettingsDialog(settingsViewModel)
        {
            Owner = this
        };
        settingsDialog.ShowDialog();
    }

    /// <summary>
    /// Handles the Exit menu item click event.
    /// </summary>
    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Navigates to the Issues view.
    /// </summary>
    private void NavigateToIssues()
    {
        CurrentViewModel = _issuesViewModel;
        StatusText.Text = "Issues";
    }

    /// <summary>
    /// Navigates to the Teams view.
    /// </summary>
    private void NavigateToTeams()
    {
        CurrentViewModel = _teamsViewModel;
        StatusText.Text = "Teams";
    }

    /// <summary>
    /// Navigates to the Users view.
    /// </summary>
    private void NavigateToUsers()
    {
        CurrentViewModel = _usersViewModel;
        StatusText.Text = "Users";
    }

    /// <summary>
    /// Handles the CurrentViewModelChanged event from the navigation service.
    /// </summary>
    private void OnCurrentViewModelChanged(object? sender, BaseViewModel viewModel)
    {
        CurrentViewModel = viewModel;
    }

    /// <summary>
    /// Updates the visual state of navigation buttons to indicate the active section.
    /// </summary>
    private void UpdateNavigationButtonStates()
    {
        // Reset all button backgrounds
        IssuesButton.Background = System.Windows.Media.Brushes.Transparent;
        TeamsButton.Background = System.Windows.Media.Brushes.Transparent;
        UsersButton.Background = System.Windows.Media.Brushes.Transparent;

        // Highlight the active button
        if (CurrentViewModel == _issuesViewModel)
        {
            IssuesButton.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x3E, 0x57, 0x71));
        }
        else if (CurrentViewModel == _teamsViewModel)
        {
            TeamsButton.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x3E, 0x57, 0x71));
        }
        else if (CurrentViewModel == _usersViewModel)
        {
            UsersButton.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x3E, 0x57, 0x71));
        }
    }

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Cleanup when the window is closing.
    /// </summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        _navigationService.CurrentViewModelChanged -= OnCurrentViewModelChanged;
        base.OnClosing(e);
    }
}
