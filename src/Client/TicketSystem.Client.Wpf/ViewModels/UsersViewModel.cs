using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
using TicketSystem.Client.Wpf.Commands;
using TicketSystem.Client.Wpf.Models;
using TicketSystem.Client.Wpf.Services;
using UserModel = TicketSystem.Client.Wpf.Models.User;

namespace TicketSystem.Client.Wpf.ViewModels;

/// <summary>
/// ViewModel for managing users in the ticket system.
/// Provides commands and properties for viewing, creating, editing, and deactivating users.
/// </summary>
/// <remarks>
/// Requirements:
/// - 10.1: THE WPF_Application SHALL display a list of all users retrieved from the UserService
/// - 10.3: THE WPF_Application SHALL provide filtering to show only Active_User accounts or only Inactive_User accounts
/// - 14.3: THE WPF_Application SHALL use ObservableCollection for list data that changes dynamically
/// </remarks>
public class UsersViewModel : BaseViewModel
{
    private readonly IUserService _userService;
    private readonly IDialogService _dialogService;

    private UserModel? _selectedUser;
    private bool _showActiveOnly;
    private bool _showInactiveOnly;
    private string _email = string.Empty;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;

    // Email validation regex pattern
    private static readonly Regex EmailRegex = new Regex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Initializes a new instance of the UsersViewModel class.
    /// </summary>
    /// <param name="userService">The user service for managing users.</param>
    /// <param name="dialogService">The dialog service for displaying confirmation dialogs.</param>
    public UsersViewModel(IUserService userService, IDialogService dialogService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        Users = new ObservableCollection<UserModel>();

        // Initialize commands
        LoadUsersCommand = new AsyncRelayCommand(
            execute: async _ => await LoadUsersAsync(),
            canExecute: _ => !IsLoading
        );

        CreateUserCommand = new AsyncRelayCommand(
            execute: async _ => await CreateUserAsync(),
            canExecute: _ => !IsLoading
        );

        EditUserCommand = new AsyncRelayCommand(
            execute: async _ => await EditUserAsync(),
            canExecute: _ => !IsLoading && SelectedUser != null
        );

        DeactivateUserCommand = new AsyncRelayCommand(
            execute: async _ => await DeactivateUserAsync(),
            canExecute: _ => !IsLoading && SelectedUser != null && SelectedUser.IsActive
        );

        ApplyFilterCommand = new AsyncRelayCommand(
            execute: async _ => await ApplyFilterAsync(),
            canExecute: _ => !IsLoading
        );

        ClearFilterCommand = new RelayCommand(
            execute: _ => ClearFilter(),
            canExecute: _ => !IsLoading
        );

        DismissErrorCommand = new RelayCommand(
            execute: _ => ErrorMessage = null
        );
    }

    /// <summary>
    /// Gets the collection of users to display.
    /// </summary>
    public ObservableCollection<UserModel> Users { get; }

    /// <summary>
    /// Gets or sets the currently selected user.
    /// </summary>
    public UserModel? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (SetProperty(ref _selectedUser, value))
            {
                // Update form fields when selection changes
                if (value != null)
                {
                    Email = value.Email;
                    FirstName = value.FirstName;
                    LastName = value.LastName;
                }

                // Notify commands that depend on SelectedUser
                RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether to show only active users.
    /// </summary>
    public bool ShowActiveOnly
    {
        get => _showActiveOnly;
        set => SetProperty(ref _showActiveOnly, value);
    }

    /// <summary>
    /// Gets or sets whether to show only inactive users.
    /// </summary>
    public bool ShowInactiveOnly
    {
        get => _showInactiveOnly;
        set => SetProperty(ref _showInactiveOnly, value);
    }

    /// <summary>
    /// Gets or sets the email for creating or editing a user.
    /// </summary>
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    /// <summary>
    /// Gets or sets the first name for creating or editing a user.
    /// </summary>
    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    /// <summary>
    /// Gets or sets the last name for creating or editing a user.
    /// </summary>
    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }

    /// <summary>
    /// Gets the command for loading users from the service.
    /// </summary>
    public ICommand LoadUsersCommand { get; }

    /// <summary>
    /// Gets the command for creating a new user.
    /// </summary>
    public ICommand CreateUserCommand { get; }

    /// <summary>
    /// Gets the command for editing an existing user.
    /// </summary>
    public ICommand EditUserCommand { get; }

    /// <summary>
    /// Gets the command for deactivating a user.
    /// </summary>
    public ICommand DeactivateUserCommand { get; }

    /// <summary>
    /// Gets the command for applying filters to the user list.
    /// </summary>
    public ICommand ApplyFilterCommand { get; }

    /// <summary>
    /// Gets the command for clearing filters.
    /// </summary>
    public ICommand ClearFilterCommand { get; }

    /// <summary>
    /// Gets the command for dismissing error messages.
    /// </summary>
    public ICommand DismissErrorCommand { get; }

    /// <summary>
    /// Loads all users from the service.
    /// </summary>
    private async Task LoadUsersAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var users = await _userService.GetAllUsersAsync();

            Users.Clear();
            foreach (var user in users)
            {
                Users.Add(user);
            }
        }
        catch (Grpc.Core.RpcException ex)
        {
            ErrorMessage = $"Failed to load users: {ex.Status.Detail}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An unexpected error occurred: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Creates a new user with the current form values.
    /// </summary>
    private async Task CreateUserAsync()
    {
        try
        {
            // Validate email
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Email is required.";
                return;
            }

            if (!EmailRegex.IsMatch(Email))
            {
                ErrorMessage = "Email format is invalid.";
                return;
            }

            IsLoading = true;
            ErrorMessage = null;

            await _userService.CreateUserAsync(Email, FirstName, LastName);

            // Refresh the user list
            await LoadUsersAsync();

            // Clear form fields
            ClearForm();
        }
        catch (Grpc.Core.RpcException ex)
        {
            ErrorMessage = $"Failed to create user: {ex.Status.Detail}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An unexpected error occurred: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Edits the currently selected user with the current form values.
    /// </summary>
    private async Task EditUserAsync()
    {
        if (SelectedUser == null)
        {
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            await _userService.UpdateUserAsync(SelectedUser.Id, FirstName, LastName);

            // Refresh the user display
            await RefreshSelectedUserAsync();
        }
        catch (Grpc.Core.RpcException ex)
        {
            ErrorMessage = $"Failed to update user: {ex.Status.Detail}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An unexpected error occurred: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Deactivates the currently selected user.
    /// </summary>
    private async Task DeactivateUserAsync()
    {
        if (SelectedUser == null)
        {
            return;
        }

        // Show confirmation dialog
        var confirmed = _dialogService.ShowConfirmation(
            $"Are you sure you want to deactivate the user '{SelectedUser.Email}'?",
            "Confirm Deactivate");

        if (!confirmed)
        {
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            await _userService.DeactivateUserAsync(SelectedUser.Id);

            // Refresh the user display
            await RefreshSelectedUserAsync();
        }
        catch (Grpc.Core.RpcException ex)
        {
            ErrorMessage = $"Failed to deactivate user: {ex.Status.Detail}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An unexpected error occurred: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Applies the current filters to the user list.
    /// </summary>
    private async Task ApplyFilterAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var users = await _userService.GetAllUsersAsync();

            // Apply active/inactive filter
            if (ShowActiveOnly)
            {
                users = users.Where(u => u.IsActive);
            }
            else if (ShowInactiveOnly)
            {
                users = users.Where(u => !u.IsActive);
            }

            Users.Clear();
            foreach (var user in users)
            {
                Users.Add(user);
            }
        }
        catch (Grpc.Core.RpcException ex)
        {
            ErrorMessage = $"Failed to filter users: {ex.Status.Detail}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An unexpected error occurred: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Refreshes the currently selected user from the service.
    /// </summary>
    private async Task RefreshSelectedUserAsync()
    {
        if (SelectedUser == null)
        {
            return;
        }

        var userId = SelectedUser.Id;
        var updatedUser = await _userService.GetUserAsync(userId);

        if (updatedUser != null)
        {
            // Find and update the user in the collection
            var index = Users.IndexOf(SelectedUser);
            if (index >= 0)
            {
                Users[index] = updatedUser;
                SelectedUser = updatedUser;
            }
        }
    }

    /// <summary>
    /// Clears all form fields.
    /// </summary>
    private void ClearForm()
    {
        Email = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    /// <summary>
    /// Raises CanExecuteChanged on all commands.
    /// </summary>
    private void RaiseCanExecuteChanged()
    {
        (LoadUsersCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (CreateUserCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (EditUserCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (DeactivateUserCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (ApplyFilterCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (ClearFilterCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Clears all filters and reloads users.
    /// </summary>
    private void ClearFilter()
    {
        ShowActiveOnly = false;
        ShowInactiveOnly = false;
        LoadUsersCommand.Execute(null);
    }
}
