using System.Collections.ObjectModel;
using System.Windows.Input;
using TicketSystem.Client.Wpf.Commands;
using TicketSystem.Client.Wpf.Models;
using TicketSystem.Client.Wpf.Services;
using IssueModel = TicketSystem.Client.Wpf.Models.Issue;

namespace TicketSystem.Client.Wpf.ViewModels;

/// <summary>
/// ViewModel for managing issues in the ticket system.
/// Provides commands and properties for viewing, creating, editing, and deleting issues.
/// </summary>
/// <remarks>
/// Requirements:
/// - 3.1: THE WPF_Application SHALL display a list of all issues retrieved from the IssueService
/// - 3.3: THE WPF_Application SHALL provide filtering options for issues by status
/// - 3.4: THE WPF_Application SHALL provide filtering options for issues by assignee (user or team)
/// - 4.1: THE WPF_Application SHALL provide a form for creating new issues
/// - 14.3: THE WPF_Application SHALL use ObservableCollection for list data that changes dynamically
/// </remarks>
public class IssuesViewModel : BaseViewModel
{
    private readonly IIssueService _issueService;
    private readonly IUserService _userService;
    private readonly ITeamService _teamService;
    private readonly IDialogService _dialogService;

    private IssueModel? _selectedIssue;
    private IssueStatus? _filterStatus;
    private string? _filterAssignee;
    private string _title = string.Empty;
    private string _description = string.Empty;
    private int _priority;
    private DateTime? _dueDate;
    private string? _selectedUserId;
    private string? _selectedTeamId;
    private IssueStatus _selectedStatus;

    /// <summary>
    /// Initializes a new instance of the IssuesViewModel class.
    /// </summary>
    /// <param name="issueService">The issue service for managing issues.</param>
    /// <param name="userService">The user service for retrieving user information.</param>
    /// <param name="teamService">The team service for retrieving team information.</param>
    /// <param name="dialogService">The dialog service for displaying confirmation dialogs.</param>
    public IssuesViewModel(
        IIssueService issueService, 
        IUserService userService, 
        ITeamService teamService,
        IDialogService dialogService)
    {
        _issueService = issueService ?? throw new ArgumentNullException(nameof(issueService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _teamService = teamService ?? throw new ArgumentNullException(nameof(teamService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        Issues = new ObservableCollection<IssueModel>();

        // Initialize commands
        LoadIssuesCommand = new AsyncRelayCommand(
            execute: async _ => await LoadIssuesAsync(),
            canExecute: _ => !IsLoading
        );

        CreateIssueCommand = new AsyncRelayCommand(
            execute: async _ => await CreateIssueAsync(),
            canExecute: _ => !IsLoading
        );

        EditIssueCommand = new AsyncRelayCommand(
            execute: async _ => await EditIssueAsync(),
            canExecute: _ => !IsLoading && SelectedIssue != null
        );

        UpdateStatusCommand = new AsyncRelayCommand(
            execute: async _ => await UpdateStatusAsync(),
            canExecute: _ => !IsLoading && SelectedIssue != null
        );

        AssignToUserCommand = new AsyncRelayCommand(
            execute: async _ => await AssignToUserAsync(),
            canExecute: _ => !IsLoading && SelectedIssue != null && !string.IsNullOrEmpty(SelectedUserId)
        );

        AssignToTeamCommand = new AsyncRelayCommand(
            execute: async _ => await AssignToTeamAsync(),
            canExecute: _ => !IsLoading && SelectedIssue != null && !string.IsNullOrEmpty(SelectedTeamId)
        );

        DeleteIssueCommand = new AsyncRelayCommand(
            execute: async _ => await DeleteIssueAsync(),
            canExecute: _ => !IsLoading && SelectedIssue != null
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
    /// Gets the collection of issues to display.
    /// </summary>
    public ObservableCollection<IssueModel> Issues { get; }

    /// <summary>
    /// Gets or sets the currently selected issue.
    /// </summary>
    public IssueModel? SelectedIssue
    {
        get => _selectedIssue;
        set
        {
            if (SetProperty(ref _selectedIssue, value))
            {
                // Update form fields when selection changes
                if (value != null)
                {
                    Title = value.Title;
                    Description = value.Description;
                    Priority = value.Priority;
                    DueDate = value.DueDate;
                    SelectedStatus = value.Status;
                }

                // Notify commands that depend on SelectedIssue
                RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the filter for issue status.
    /// </summary>
    public IssueStatus? FilterStatus
    {
        get => _filterStatus;
        set => SetProperty(ref _filterStatus, value);
    }

    /// <summary>
    /// Gets or sets the filter for issue assignee (user or team ID).
    /// </summary>
    public string? FilterAssignee
    {
        get => _filterAssignee;
        set => SetProperty(ref _filterAssignee, value);
    }

    /// <summary>
    /// Gets or sets the title for creating or editing an issue.
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <summary>
    /// Gets or sets the description for creating or editing an issue.
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    /// <summary>
    /// Gets or sets the priority for creating or editing an issue.
    /// </summary>
    public int Priority
    {
        get => _priority;
        set => SetProperty(ref _priority, value);
    }

    /// <summary>
    /// Gets or sets the due date for creating or editing an issue.
    /// </summary>
    public DateTime? DueDate
    {
        get => _dueDate;
        set => SetProperty(ref _dueDate, value);
    }

    /// <summary>
    /// Gets or sets the selected user ID for assignment operations.
    /// </summary>
    public string? SelectedUserId
    {
        get => _selectedUserId;
        set
        {
            if (SetProperty(ref _selectedUserId, value))
            {
                RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the selected team ID for assignment operations.
    /// </summary>
    public string? SelectedTeamId
    {
        get => _selectedTeamId;
        set
        {
            if (SetProperty(ref _selectedTeamId, value))
            {
                RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the selected status for status update operations.
    /// </summary>
    public IssueStatus SelectedStatus
    {
        get => _selectedStatus;
        set => SetProperty(ref _selectedStatus, value);
    }

    /// <summary>
    /// Gets the command for loading issues from the service.
    /// </summary>
    public ICommand LoadIssuesCommand { get; }

    /// <summary>
    /// Gets the command for creating a new issue.
    /// </summary>
    public ICommand CreateIssueCommand { get; }

    /// <summary>
    /// Gets the command for editing an existing issue.
    /// </summary>
    public ICommand EditIssueCommand { get; }

    /// <summary>
    /// Gets the command for updating an issue's status.
    /// </summary>
    public ICommand UpdateStatusCommand { get; }

    /// <summary>
    /// Gets the command for assigning an issue to a user.
    /// </summary>
    public ICommand AssignToUserCommand { get; }

    /// <summary>
    /// Gets the command for assigning an issue to a team.
    /// </summary>
    public ICommand AssignToTeamCommand { get; }

    /// <summary>
    /// Gets the command for deleting an issue.
    /// </summary>
    public ICommand DeleteIssueCommand { get; }

    /// <summary>
    /// Gets the command for applying filters to the issue list.
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
    /// Loads all issues from the service.
    /// </summary>
    private async Task LoadIssuesAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var issues = await _issueService.GetAllIssuesAsync();

            Issues.Clear();
            foreach (var issue in issues)
            {
                Issues.Add(issue);
            }
        }
        catch (Grpc.Core.RpcException ex)
        {
            ErrorMessage = $"Failed to load issues: {ex.Status.Detail}";
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
    /// Creates a new issue with the current form values.
    /// </summary>
    private async Task CreateIssueAsync()
    {
        try
        {
            // Validate title
            if (string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Title is required.";
                return;
            }

            IsLoading = true;
            ErrorMessage = null;

            await _issueService.CreateIssueAsync(Title, Description, Priority, DueDate);

            // Refresh the issue list
            await LoadIssuesAsync();

            // Clear form fields
            ClearForm();
        }
        catch (Grpc.Core.RpcException ex)
        {
            ErrorMessage = $"Failed to create issue: {ex.Status.Detail}";
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
    /// Edits the currently selected issue with the current form values.
    /// </summary>
    private async Task EditIssueAsync()
    {
        if (SelectedIssue == null)
        {
            return;
        }

        try
        {
            // Validate title
            if (string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Title is required.";
                return;
            }

            IsLoading = true;
            ErrorMessage = null;

            await _issueService.UpdateIssueAsync(SelectedIssue.Id, Title, Description, Priority, DueDate);

            // Refresh the issue display
            await RefreshSelectedIssueAsync();
        }
        catch (Grpc.Core.RpcException ex)
        {
            ErrorMessage = $"Failed to update issue: {ex.Status.Detail}";
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
    /// Updates the status of the currently selected issue.
    /// </summary>
    private async Task UpdateStatusAsync()
    {
        if (SelectedIssue == null)
        {
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            await _issueService.UpdateIssueStatusAsync(SelectedIssue.Id, SelectedStatus);

            // Refresh the issue display
            await RefreshSelectedIssueAsync();
        }
        catch (Grpc.Core.RpcException ex)
        {
            ErrorMessage = $"Failed to update status: {ex.Status.Detail}";
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
    /// Assigns the currently selected issue to a user.
    /// </summary>
    private async Task AssignToUserAsync()
    {
        if (SelectedIssue == null || string.IsNullOrEmpty(SelectedUserId))
        {
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            await _issueService.AssignIssueToUserAsync(SelectedIssue.Id, SelectedUserId);

            // Refresh the issue display
            await RefreshSelectedIssueAsync();
        }
        catch (Grpc.Core.RpcException ex)
        {
            ErrorMessage = $"Failed to assign issue to user: {ex.Status.Detail}";
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
    /// Assigns the currently selected issue to a team.
    /// </summary>
    private async Task AssignToTeamAsync()
    {
        if (SelectedIssue == null || string.IsNullOrEmpty(SelectedTeamId))
        {
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            await _issueService.AssignIssueToTeamAsync(SelectedIssue.Id, SelectedTeamId);

            // Refresh the issue display
            await RefreshSelectedIssueAsync();
        }
        catch (Grpc.Core.RpcException ex)
        {
            ErrorMessage = $"Failed to assign issue to team: {ex.Status.Detail}";
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
    /// Deletes the currently selected issue.
    /// </summary>
    private async Task DeleteIssueAsync()
    {
        if (SelectedIssue == null)
        {
            return;
        }

        // Show confirmation dialog
        var confirmed = _dialogService.ShowConfirmation(
            $"Are you sure you want to delete the issue '{SelectedIssue.Title}'?",
            "Confirm Delete");

        if (!confirmed)
        {
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            await _issueService.DeleteIssueAsync(SelectedIssue.Id);

            // Remove from collection
            Issues.Remove(SelectedIssue);
            SelectedIssue = null;
        }
        catch (Grpc.Core.RpcException ex)
        {
            ErrorMessage = $"Failed to delete issue: {ex.Status.Detail}";
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
    /// Applies the current filters to the issue list.
    /// </summary>
    private async Task ApplyFilterAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            IEnumerable<IssueModel> issues;

            // Check if filtering by assignee
            if (!string.IsNullOrEmpty(FilterAssignee))
            {
                // Try to determine if it's a user or team ID
                // For simplicity, we'll try both and use whichever returns results
                var userIssues = await _issueService.GetIssuesByUserAsync(FilterAssignee);
                var teamIssues = await _issueService.GetIssuesByTeamAsync(FilterAssignee);

                issues = userIssues.Any() ? userIssues : teamIssues;
            }
            else
            {
                issues = await _issueService.GetAllIssuesAsync();
            }

            // Apply status filter if specified
            if (FilterStatus.HasValue)
            {
                issues = issues.Where(i => i.Status == FilterStatus.Value);
            }

            Issues.Clear();
            foreach (var issue in issues)
            {
                Issues.Add(issue);
            }
        }
        catch (Grpc.Core.RpcException ex)
        {
            ErrorMessage = $"Failed to filter issues: {ex.Status.Detail}";
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
    /// Refreshes the currently selected issue from the service.
    /// </summary>
    private async Task RefreshSelectedIssueAsync()
    {
        if (SelectedIssue == null)
        {
            return;
        }

        var issueId = SelectedIssue.Id;
        var updatedIssue = await _issueService.GetIssueAsync(issueId);

        if (updatedIssue != null)
        {
            // Find and update the issue in the collection
            var index = Issues.IndexOf(SelectedIssue);
            if (index >= 0)
            {
                Issues[index] = updatedIssue;
                SelectedIssue = updatedIssue;
            }
        }
    }

    /// <summary>
    /// Clears all form fields.
    /// </summary>
    private void ClearForm()
    {
        Title = string.Empty;
        Description = string.Empty;
        Priority = 0;
        DueDate = null;
        SelectedStatus = IssueStatus.Open;
        SelectedUserId = null;
        SelectedTeamId = null;
    }

    /// <summary>
    /// Clears all filters and reloads issues.
    /// </summary>
    private void ClearFilter()
    {
        FilterStatus = null;
        FilterAssignee = null;
        LoadIssuesCommand.Execute(null);
    }

    /// <summary>
    /// Raises CanExecuteChanged on all commands.
    /// </summary>
    private void RaiseCanExecuteChanged()
    {
        (LoadIssuesCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (CreateIssueCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (EditIssueCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (UpdateStatusCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (AssignToUserCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (AssignToTeamCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (DeleteIssueCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (ApplyFilterCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
    }
}
