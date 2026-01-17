using TicketSystem.Issue.Domain;
using TicketSystem.Team.Contracts;
using TicketSystem.User.Contracts;

namespace TicketSystem.Issue.Application;

internal class IssueService
{
    private readonly IIssueRepository _issueRepository;
    private readonly IUserModuleApi _userModuleApi;
    private readonly ITeamModuleApi _teamModuleApi;

    public IssueService(
        IIssueRepository issueRepository,
        IUserModuleApi userModuleApi,
        ITeamModuleApi teamModuleApi)
    {
        _issueRepository = issueRepository;
        _userModuleApi = userModuleApi;
        _teamModuleApi = teamModuleApi;
    }

    public async Task<long> CreateIssueAsync(string title, string? description, IssuePriority priority, DateTime? dueDate)
    {
        var issue = new IssueBusinessEntity(0, title, description, priority, dueDate);

        await _issueRepository.AddAsync(issue);

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

        issue.Update(title, description, priority, dueDate);
        await _issueRepository.UpdateAsync(issue);
    }

    public async Task AssignIssueToUserAsync(long issueId, long? userId)
    {
        var issue = await _issueRepository.GetByIdAsync(issueId);
        if (issue == null)
            throw new KeyNotFoundException($"Issue with ID '{issueId}' not found.");

        if (userId.HasValue)
        {
            var userExists = await _userModuleApi.UserExistsAsync(new UserExistsRequest { UserId = userId.Value });
            if (!userExists)
                throw new InvalidOperationException($"User with ID '{userId}' does not exist.");
        }

        issue.AssignToUser(userId);
        await _issueRepository.UpdateAsync(issue);
    }

    public async Task AssignIssueToTeamAsync(long issueId, long? teamId)
    {
        var issue = await _issueRepository.GetByIdAsync(issueId);
        if (issue == null)
            throw new KeyNotFoundException($"Issue with ID '{issueId}' not found.");

        if (teamId.HasValue)
        {
            var teamExists = await _teamModuleApi.TeamExistsAsync(new TeamExistsRequest { TeamId = teamId.Value });
            if (!teamExists)
                throw new InvalidOperationException($"Team with ID '{teamId}' does not exist.");
        }

        issue.AssignToTeam(teamId);
        await _issueRepository.UpdateAsync(issue);
    }

    public async Task UpdateIssueStatusAsync(long issueId, IssueStatus status)
    {
        var issue = await _issueRepository.GetByIdAsync(issueId);
        if (issue == null)
            throw new KeyNotFoundException($"Issue with ID '{issueId}' not found.");

        issue.UpdateStatus(status);
        await _issueRepository.UpdateAsync(issue);
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
        var issue = await _issueRepository.GetByIdAsync(issueId);
        if (issue == null)
            throw new KeyNotFoundException($"Issue with ID '{issueId}' not found.");

        await _issueRepository.DeleteAsync(issue);
    }

    public async Task<bool> IssueExistsAsync(long issueId)
    {
        return await _issueRepository.ExistsAsync(issueId);
    }
}
