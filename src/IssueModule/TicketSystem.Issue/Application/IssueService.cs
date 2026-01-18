using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TicketSystem.Issue.Configuration;
using TicketSystem.Issue.Domain;
using TicketSystem.Team.Contracts;
using TicketSystem.User.Contracts;

namespace TicketSystem.Issue.Application;

internal class IssueService
{
    private readonly IIssueRepository _issueRepository;
    private readonly IUserModuleApi _userModuleApi;
    private readonly ITeamModuleApi _teamModuleApi;
    private readonly ILogger<IssueService> _logger;
    private readonly IOptionsMonitor<IssueSettings> _settings;

    public IssueService(
        IIssueRepository issueRepository,
        IUserModuleApi userModuleApi,
        ITeamModuleApi teamModuleApi,
        ILogger<IssueService> logger,
        IOptionsMonitor<IssueSettings> settings)
    {
        _issueRepository = issueRepository;
        _userModuleApi = userModuleApi;
        _teamModuleApi = teamModuleApi;
        _logger = logger;
        _settings = settings;
    }

    public async Task<long> CreateIssueAsync(string title, string? description, IssuePriority priority, DateTime? dueDate)
    {
        _logger.LogInformation("Creating issue with title: {Title}, Priority: {Priority}", title, priority);
        
        // Get current settings
        var settings = _settings.CurrentValue;
        
        // Validate title length
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        
        if (title.Length < settings.MinTitleLength)
            throw new ArgumentException($"Title must be at least {settings.MinTitleLength} characters long.", nameof(title));
        
        if (title.Length > settings.MaxTitleLength)
            throw new ArgumentException($"Title cannot exceed {settings.MaxTitleLength} characters.", nameof(title));
        
        // Validate description length if provided
        if (!string.IsNullOrEmpty(description) && description.Length > settings.MaxDescriptionLength)
            throw new ArgumentException($"Description cannot exceed {settings.MaxDescriptionLength} characters.", nameof(description));
        
        // Validate due date
        if (!settings.AllowNoDueDate && !dueDate.HasValue)
            throw new ArgumentException("Due date is required based on current settings.", nameof(dueDate));
        
        var issue = new IssueBusinessEntity(0, title, description, priority, dueDate);

        await _issueRepository.AddAsync(issue);

        _logger.LogInformation("Issue created successfully with ID: {IssueId}, Title: {Title}", issue.Id, title);
        return issue.Id;
    }

    public async Task<IssueBusinessEntity> GetIssueAsync(long issueId)
    {
        var issue = await _issueRepository.GetByIdAsync(issueId);
        if (issue == null)
            throw new KeyNotFoundException($"Issue with ID '{issueId}' not found.");

        return issue;
    }

    public async Task<IReadOnlyList<IssueBusinessEntity>> GetAllIssuesAsync()
    {
        return await _issueRepository.GetAllAsync();
    }

    public async Task UpdateIssueAsync(long issueId, string title, string? description, IssuePriority priority, DateTime? dueDate)
    {
        var issue = await _issueRepository.GetByIdAsync(issueId);
        if (issue == null)
            throw new KeyNotFoundException($"Issue with ID '{issueId}' not found.");
        
        // Get current settings
        var settings = _settings.CurrentValue;
        
        // Validate title length
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        
        if (title.Length < settings.MinTitleLength)
            throw new ArgumentException($"Title must be at least {settings.MinTitleLength} characters long.", nameof(title));
        
        if (title.Length > settings.MaxTitleLength)
            throw new ArgumentException($"Title cannot exceed {settings.MaxTitleLength} characters.", nameof(title));
        
        // Validate description length if provided
        if (!string.IsNullOrEmpty(description) && description.Length > settings.MaxDescriptionLength)
            throw new ArgumentException($"Description cannot exceed {settings.MaxDescriptionLength} characters.", nameof(description));
        
        // Validate due date
        if (!settings.AllowNoDueDate && !dueDate.HasValue)
            throw new ArgumentException("Due date is required based on current settings.", nameof(dueDate));

        issue.Update(title, description, priority, dueDate);
        await _issueRepository.UpdateAsync(issue);
    }

    public async Task AssignIssueToUserAsync(long issueId, long? userId)
    {
        _logger.LogInformation("Assigning issue {IssueId} to user {UserId}", issueId, userId?.ToString() ?? "(unassigned)");
        
        var issue = await _issueRepository.GetByIdAsync(issueId);
        if (issue == null)
        {
            _logger.LogWarning("Assign to user failed - Issue not found: {IssueId}", issueId);
            throw new KeyNotFoundException($"Issue with ID '{issueId}' not found.");
        }

        if (userId.HasValue)
        {
            _logger.LogDebug("Validating user existence via User module: {UserId}", userId.Value);
            var userExists = await _userModuleApi.UserExistsAsync(new UserExistsRequest { UserId = userId.Value });
            if (!userExists)
            {
                _logger.LogWarning("Assign to user failed - User does not exist: {UserId}", userId.Value);
                throw new InvalidOperationException($"User with ID '{userId}' does not exist.");
            }
        }

        issue.AssignToUser(userId);
        await _issueRepository.UpdateAsync(issue);
        
        _logger.LogInformation("Issue {IssueId} assigned to user {UserId} successfully", issueId, userId?.ToString() ?? "(unassigned)");
    }

    public async Task AssignIssueToTeamAsync(long issueId, long? teamId)
    {
        _logger.LogInformation("Assigning issue {IssueId} to team {TeamId}", issueId, teamId?.ToString() ?? "(unassigned)");
        
        var issue = await _issueRepository.GetByIdAsync(issueId);
        if (issue == null)
        {
            _logger.LogWarning("Assign to team failed - Issue not found: {IssueId}", issueId);
            throw new KeyNotFoundException($"Issue with ID '{issueId}' not found.");
        }

        if (teamId.HasValue)
        {
            _logger.LogDebug("Validating team existence via Team module: {TeamId}", teamId.Value);
            var teamExists = await _teamModuleApi.TeamExistsAsync(new TeamExistsRequest { TeamId = teamId.Value });
            if (!teamExists)
            {
                _logger.LogWarning("Assign to team failed - Team does not exist: {TeamId}", teamId.Value);
                throw new InvalidOperationException($"Team with ID '{teamId}' does not exist.");
            }
        }

        issue.AssignToTeam(teamId);
        await _issueRepository.UpdateAsync(issue);
        
        _logger.LogInformation("Issue {IssueId} assigned to team {TeamId} successfully", issueId, teamId?.ToString() ?? "(unassigned)");
    }

    public async Task UpdateIssueStatusAsync(long issueId, IssueStatus status)
    {
        _logger.LogInformation("Updating issue {IssueId} status to {Status}", issueId, status);
        
        var issue = await _issueRepository.GetByIdAsync(issueId);
        if (issue == null)
        {
            _logger.LogWarning("Update status failed - Issue not found: {IssueId}", issueId);
            throw new KeyNotFoundException($"Issue with ID '{issueId}' not found.");
        }

        var oldStatus = issue.Status;
        issue.UpdateStatus(status);
        await _issueRepository.UpdateAsync(issue);
        
        _logger.LogInformation("Issue {IssueId} status updated from {OldStatus} to {NewStatus}", issueId, oldStatus, status);
    }

    public async Task<IReadOnlyList<IssueBusinessEntity>> GetIssuesByUserAsync(long userId)
    {
        return await _issueRepository.GetByUserIdAsync(userId);
    }

    public async Task<IReadOnlyList<IssueBusinessEntity>> GetIssuesByTeamAsync(long teamId)
    {
        return await _issueRepository.GetByTeamIdAsync(teamId);
    }

    public async Task DeleteIssueAsync(long issueId)
    {
        _logger.LogInformation("Deleting issue {IssueId}", issueId);
        
        var issue = await _issueRepository.GetByIdAsync(issueId);
        if (issue == null)
        {
            _logger.LogWarning("Delete failed - Issue not found: {IssueId}", issueId);
            throw new KeyNotFoundException($"Issue with ID '{issueId}' not found.");
        }

        await _issueRepository.DeleteAsync(issue);
        
        _logger.LogInformation("Issue {IssueId} deleted successfully", issueId);
    }

    public async Task<bool> IssueExistsAsync(long issueId)
    {
        return await _issueRepository.ExistsAsync(issueId);
    }
}
