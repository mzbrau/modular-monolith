using TicketSystem.Issue.Application;
using TicketSystem.Issue.Contracts;
using TicketSystem.Issue.Domain;

namespace TicketSystem.Issue.Adapters;

internal class IssueModuleApiAdapter : IIssueModuleApi
{
    private readonly IssueService _issueService;

    public IssueModuleApiAdapter(IssueService issueService)
    {
        _issueService = issueService;
    }

    public async Task<long> CreateIssueAsync(string title, string? description, int priority, DateTime? dueDate)
    {
        var issueId = await _issueService.CreateIssueAsync(title, description, (IssuePriority)priority, dueDate);
        return issueId.Value;
    }

    public async Task<IssueDataContract?> GetIssueAsync(long issueId)
    {
        try
        {
            var issue = await _issueService.GetIssueAsync(new IssueId(issueId));
            return IssueConverter.ToDataContract(issue);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    public async Task<List<IssueDataContract>> GetAllIssuesAsync()
    {
        var issues = await _issueService.GetAllIssuesAsync();
        return issues.Select(IssueConverter.ToDataContract).ToList();
    }

    public async Task UpdateIssueAsync(long issueId, string title, string? description, int priority, DateTime? dueDate)
    {
        await _issueService.UpdateIssueAsync(new IssueId(issueId), title, description, (IssuePriority)priority, dueDate);
    }

    public async Task AssignIssueToUserAsync(long issueId, long? userId)
    {
        await _issueService.AssignIssueToUserAsync(new IssueId(issueId), userId);
    }

    public async Task AssignIssueToTeamAsync(long issueId, long? teamId)
    {
        await _issueService.AssignIssueToTeamAsync(new IssueId(issueId), teamId);
    }

    public async Task UpdateIssueStatusAsync(long issueId, int status)
    {
        await _issueService.UpdateIssueStatusAsync(new IssueId(issueId), (IssueStatus)status);
    }

    public async Task<List<IssueDataContract>> GetIssuesByUserAsync(long userId)
    {
        var issues = await _issueService.GetIssuesByUserAsync(userId);
        return issues.Select(IssueConverter.ToDataContract).ToList();
    }

    public async Task<List<IssueDataContract>> GetIssuesByTeamAsync(long teamId)
    {
        var issues = await _issueService.GetIssuesByTeamAsync(teamId);
        return issues.Select(IssueConverter.ToDataContract).ToList();
    }

    public async Task DeleteIssueAsync(long issueId)
    {
        await _issueService.DeleteIssueAsync(new IssueId(issueId));
    }
}
