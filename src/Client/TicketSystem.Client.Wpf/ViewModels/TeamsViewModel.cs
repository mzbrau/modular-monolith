using System.Collections.ObjectModel;
using System.Windows.Input;
using TicketSystem.Client.Wpf.Commands;
using TicketSystem.Client.Wpf.Models;
using TicketSystem.Client.Wpf.Services;
using TeamModel = TicketSystem.Client.Wpf.Models.Team;

namespace TicketSystem.Client.Wpf.ViewModels;

/// <summary>
/// ViewModel for managing teams in the ticket system.
/// Provides commands and properties for viewing, creating, editing teams and managing team members.
/// </summary>
/// <remarks>
/// Requirements:
/// - 7.1: THE WPF_Application SHALL display a list of all teams retrieved from the TeamService
/// - 7.3: WHEN a user selects a team from the list, THE WPF_Application SHALL display detailed information including all team members
/// - 14.3: THE WPF_Application SHALL use ObservableCollection for list data that changes dynamically
/// </remarks>
public class TeamsViewModel : BaseViewModel
{
    private readonly ITeamService _teamService;
    private readonly IUserService _userService;

    private TeamModel? _selectedTeam;
    private string _teamName = string.Empty;
    private string _teamDescription = string.Empty;
    private string? _selectedUserId;
    private int _selectedRole;

    /// <summary>
    /// Initializes a new instance of the TeamsViewModel class.
    /// </summary>
    /// <param name="teamService">The team service for managing teams.</param>
    /// <param name="userService">The user service for retrieving user information.</param>
    public TeamsViewModel(ITeamService teamService, IUserService userService)
    {
        _teamService = teamService ?? throw new ArgumentNullException(nameof(teamService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));

        Teams = new ObservableCollection<TeamModel>();
        TeamMembers = new ObservableCollection<TeamMember>();

        // Initialize commands
        LoadTeamsCommand = new AsyncRelayCommand(
            execute: async _ => await LoadTeamsAsync(),
            canExecute: _ => !IsLoading
        );

        LoadTeamMembersCommand = new AsyncRelayCommand(
            execute: async _ => await LoadTeamMembersAsync(),
            canExecute: _ => !IsLoading && SelectedTeam != null
        );

        CreateTeamCommand = new AsyncRelayCommand(
            execute: async _ => await CreateTeamAsync(),
            canExecute: _ => !IsLoading
        );

        EditTeamCommand = new AsyncRelayCommand(
            execute: async _ => await EditTeamAsync(),
            canExecute: _ => !IsLoading && SelectedTeam != null
        );

        AddMemberCommand = new AsyncRelayCommand(
            execute: async _ => await AddMemberAsync(),
            canExecute: _ => !IsLoading && SelectedTeam != null && !string.IsNullOrEmpty(SelectedUserId)
        );

        RemoveMemberCommand = new AsyncRelayCommand(
            execute: async parameter => await RemoveMemberAsync(parameter),
            canExecute: _ => !IsLoading && SelectedTeam != null
        );

        DismissErrorCommand = new RelayCommand(
            execute: _ => ErrorMessage = null
        );
    }

    /// <summary>
    /// Gets the collection of teams to display.
    /// </summary>
    public ObservableCollection<TeamModel> Teams { get; }

    /// <summary>
    /// Gets the collection of team members for the selected team.
    /// </summary>
    public ObservableCollection<TeamMember> TeamMembers { get; }

    /// <summary>
    /// Gets or sets the currently selected team.
    /// </summary>
    public TeamModel? SelectedTeam
    {
        get => _selectedTeam;
        set
        {
            if (SetProperty(ref _selectedTeam, value))
            {
                // Update form fields when selection changes
                if (value != null)
                {
                    TeamName = value.Name;
                    TeamDescription = value.Description;
                    
                    // Load team members for the selected team
                    _ = LoadTeamMembersAsync();
                }
                else
                {
                    TeamMembers.Clear();
                }

                // Notify commands that depend on SelectedTeam
                RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the team name for creating or editing a team.
    /// </summary>
    public string TeamName
    {
        get => _teamName;
        set => SetProperty(ref _teamName, value);
    }

    /// <summary>
    /// Gets or sets the team description for creating or editing a team.
    /// </summary>
    public string TeamDescription
    {
        get => _teamDescription;
        set => SetProperty(ref _teamDescription, value);
    }

    /// <summary>
    /// Gets or sets the selected user ID for adding members to a team.
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
    /// Gets or sets the selected role for adding members to a team.
    /// </summary>
    public int SelectedRole
    {
        get => _selectedRole;
        set => SetProperty(ref _selectedRole, value);
    }

    /// <summary>
    /// Gets the command for loading teams from the service.
    /// </summary>
    public ICommand LoadTeamsCommand { get; }

    /// <summary>
    /// Gets the command for loading team members for the selected team.
    /// </summary>
    public ICommand LoadTeamMembersCommand { get; }

    /// <summary>
    /// Gets the command for creating a new team.
    /// </summary>
    public ICommand CreateTeamCommand { get; }

    /// <summary>
    /// Gets the command for editing an existing team.
    /// </summary>
    public ICommand EditTeamCommand { get; }

    /// <summary>
    /// Gets the command for adding a member to a team.
    /// </summary>
    public ICommand AddMemberCommand { get; }

    /// <summary>
    /// Gets the command for removing a member from a team.
    /// </summary>
    public ICommand RemoveMemberCommand { get; }

    /// <summary>
    /// Gets the command for dismissing error messages.
    /// </summary>
    public ICommand DismissErrorCommand { get; }

    /// <summary>
    /// Loads all teams from the service.
    /// </summary>
    private async Task LoadTeamsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var teams = await _teamService.GetAllTeamsAsync();

            Teams.Clear();
            foreach (var team in teams)
            {
                Teams.Add(team);
            }
        }
        catch (Grpc.Core.RpcException ex)
        {
            ErrorMessage = $"Failed to load teams: {ex.Status.Detail}";
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
    /// Loads team members for the currently selected team.
    /// </summary>
    private async Task LoadTeamMembersAsync()
    {
        if (SelectedTeam == null)
        {
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var members = await _teamService.GetTeamMembersAsync(SelectedTeam.Id);

            TeamMembers.Clear();
            foreach (var member in members)
            {
                TeamMembers.Add(member);
            }
        }
        catch (Grpc.Core.RpcException ex)
        {
            ErrorMessage = $"Failed to load team members: {ex.Status.Detail}";
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
    /// Creates a new team with the current form values.
    /// </summary>
    private async Task CreateTeamAsync()
    {
        try
        {
            // Validate team name
            if (string.IsNullOrWhiteSpace(TeamName))
            {
                ErrorMessage = "Team name is required.";
                return;
            }

            IsLoading = true;
            ErrorMessage = null;

            await _teamService.CreateTeamAsync(TeamName, TeamDescription);

            // Refresh the team list
            await LoadTeamsAsync();

            // Clear form fields
            ClearForm();
        }
        catch (Grpc.Core.RpcException ex)
        {
            ErrorMessage = $"Failed to create team: {ex.Status.Detail}";
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
    /// Edits the currently selected team with the current form values.
    /// </summary>
    private async Task EditTeamAsync()
    {
        if (SelectedTeam == null)
        {
            return;
        }

        try
        {
            // Validate team name
            if (string.IsNullOrWhiteSpace(TeamName))
            {
                ErrorMessage = "Team name is required.";
                return;
            }

            IsLoading = true;
            ErrorMessage = null;

            await _teamService.UpdateTeamAsync(SelectedTeam.Id, TeamName, TeamDescription);

            // Refresh the team display
            await RefreshSelectedTeamAsync();
        }
        catch (Grpc.Core.RpcException ex)
        {
            ErrorMessage = $"Failed to update team: {ex.Status.Detail}";
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
    /// Adds a member to the currently selected team.
    /// </summary>
    private async Task AddMemberAsync()
    {
        if (SelectedTeam == null || string.IsNullOrEmpty(SelectedUserId))
        {
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            await _teamService.AddMemberToTeamAsync(SelectedTeam.Id, SelectedUserId, SelectedRole);

            // Refresh the team member list
            await LoadTeamMembersAsync();

            // Clear selection
            SelectedUserId = null;
            SelectedRole = 0;
        }
        catch (Grpc.Core.RpcException ex)
        {
            ErrorMessage = $"Failed to add member to team: {ex.Status.Detail}";
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
    /// Removes a member from the currently selected team.
    /// </summary>
    private async Task RemoveMemberAsync(object? parameter)
    {
        if (SelectedTeam == null || parameter is not string userId)
        {
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            await _teamService.RemoveMemberFromTeamAsync(SelectedTeam.Id, userId);

            // Refresh the team member list
            await LoadTeamMembersAsync();
        }
        catch (Grpc.Core.RpcException ex)
        {
            ErrorMessage = $"Failed to remove member from team: {ex.Status.Detail}";
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
    /// Refreshes the currently selected team from the service.
    /// </summary>
    private async Task RefreshSelectedTeamAsync()
    {
        if (SelectedTeam == null)
        {
            return;
        }

        var teamId = SelectedTeam.Id;
        var updatedTeam = await _teamService.GetTeamAsync(teamId);

        if (updatedTeam != null)
        {
            // Find and update the team in the collection
            var index = Teams.IndexOf(SelectedTeam);
            if (index >= 0)
            {
                Teams[index] = updatedTeam;
                SelectedTeam = updatedTeam;
            }
        }
    }

    /// <summary>
    /// Clears all form fields.
    /// </summary>
    private void ClearForm()
    {
        TeamName = string.Empty;
        TeamDescription = string.Empty;
        SelectedUserId = null;
        SelectedRole = 0;
    }

    /// <summary>
    /// Raises CanExecuteChanged on all commands.
    /// </summary>
    private void RaiseCanExecuteChanged()
    {
        (LoadTeamsCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (LoadTeamMembersCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (CreateTeamCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (EditTeamCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (AddMemberCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (RemoveMemberCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
    }
}
