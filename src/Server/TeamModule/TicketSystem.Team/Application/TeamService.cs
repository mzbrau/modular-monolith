using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TicketSystem.Team.Application.Configuration;
using TicketSystem.Team.Domain;
using TicketSystem.User.Contracts;

namespace TicketSystem.Team.Application;

internal class TeamService
{
    private readonly ITeamRepository _teamRepository;
    private readonly IUserModuleApi _userModuleApi;
    private readonly ILogger<TeamService> _logger;
    private readonly IOptionsMonitor<TeamSettings> _settings;

    public TeamService(
        ITeamRepository teamRepository, 
        IUserModuleApi userModuleApi, 
        ILogger<TeamService> logger,
        IOptionsMonitor<TeamSettings> settings)
    {
        _teamRepository = teamRepository;
        _userModuleApi = userModuleApi;
        _logger = logger;
        _settings = settings;
    }

    public async Task<long> CreateTeamAsync(string name, string? description)
    {
        _logger.LogInformation("Creating team with name: {TeamName}", name);
        
        // Get current settings
        var settings = _settings.CurrentValue;
        
        // Validate name length
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        
        if (name.Length < settings.MinNameLength)
            throw new ArgumentException($"Name must be at least {settings.MinNameLength} characters long.", nameof(name));
        
        if (name.Length > settings.MaxNameLength)
            throw new ArgumentException($"Name cannot exceed {settings.MaxNameLength} characters.", nameof(name));
        
        // Validate description length if provided
        if (!string.IsNullOrEmpty(description) && description.Length > settings.MaxDescriptionLength)
            throw new ArgumentException($"Description cannot exceed {settings.MaxDescriptionLength} characters.", nameof(description));
        
        var team = new TeamBusinessEntity(0, name, description);
        
        await _teamRepository.AddAsync(team);
        
        _logger.LogInformation("Team created successfully with ID: {TeamId}, Name: {TeamName}", team.Id, name);
        return team.Id;
    }

    public async Task<TeamBusinessEntity> GetTeamAsync(long teamId)
    {
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
            throw new KeyNotFoundException($"Team with ID '{teamId}' not found.");
        
        return team;
    }

    public async Task<IReadOnlyList<TeamBusinessEntity>> GetAllTeamsAsync()
    {
        return await _teamRepository.GetAllAsync();
    }

    public async Task UpdateTeamAsync(long teamId, string name, string? description)
    {
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
            throw new KeyNotFoundException($"Team with ID '{teamId}' not found.");
        
        // Get current settings
        var settings = _settings.CurrentValue;
        
        // Validate name length
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        
        if (name.Length < settings.MinNameLength)
            throw new ArgumentException($"Name must be at least {settings.MinNameLength} characters long.", nameof(name));
        
        if (name.Length > settings.MaxNameLength)
            throw new ArgumentException($"Name cannot exceed {settings.MaxNameLength} characters.", nameof(name));
        
        // Validate description length if provided
        if (!string.IsNullOrEmpty(description) && description.Length > settings.MaxDescriptionLength)
            throw new ArgumentException($"Description cannot exceed {settings.MaxDescriptionLength} characters.", nameof(description));

        team.Update(name, description);
        await _teamRepository.UpdateAsync(team);
    }

    public async Task AddMemberToTeamAsync(long teamId, long userId, TeamRole role = TeamRole.Member)
    {
        _logger.LogInformation("Adding user {UserId} to team {TeamId} with role {Role}", userId, teamId, role);
        
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
        {
            _logger.LogWarning("Add member failed - Team not found: {TeamId}", teamId);
            throw new KeyNotFoundException($"Team with ID '{teamId}' not found.");
        }
        
        // Validate max team members
        var settings = _settings.CurrentValue;
        if (team.Members.Count >= settings.MaxTeamMembers)
        {
            _logger.LogWarning("Add member failed - Team has reached maximum members: {TeamId}", teamId);
            throw new InvalidOperationException($"Team has reached the maximum number of members ({settings.MaxTeamMembers}).");
        }

        // Validate user exists via User module
        _logger.LogDebug("Validating user existence via User module: {UserId}", userId);
        var userExists = await _userModuleApi.UserExistsAsync(new UserExistsRequest { UserId = userId });
        if (!userExists)
        {
            _logger.LogWarning("Add member failed - User does not exist: {UserId}", userId);
            throw new InvalidOperationException($"User with ID '{userId}' does not exist.");
        }

        team.AddMember(userId, role);
        await _teamRepository.UpdateAsync(team);
        
        _logger.LogInformation("User {UserId} added to team {TeamId} successfully", userId, teamId);
    }

    public async Task RemoveMemberFromTeamAsync(long teamId, long userId)
    {
        _logger.LogInformation("Removing user {UserId} from team {TeamId}", userId, teamId);
        
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
        {
            _logger.LogWarning("Remove member failed - Team not found: {TeamId}", teamId);
            throw new KeyNotFoundException($"Team with ID '{teamId}' not found.");
        }

        team.RemoveMember(userId);
        await _teamRepository.UpdateAsync(team);
        
        _logger.LogInformation("User {UserId} removed from team {TeamId} successfully", userId, teamId);
    }

    public async Task<IReadOnlyList<TeamMemberBusinessEntity>> GetTeamMembersAsync(long teamId)
    {
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
            throw new KeyNotFoundException($"Team with ID '{teamId}' not found.");

        return team.Members;
    }

    public async Task<bool> TeamExistsAsync(long teamId)
    {
        return await _teamRepository.ExistsAsync(teamId);
    }

    public async Task<IReadOnlyList<long>> GetTeamMemberIdsAsync(long teamId)
    {
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
            throw new KeyNotFoundException($"Team with ID '{teamId}' not found.");

        return team.Members.Select(m => m.UserId).ToList();
    }
}
