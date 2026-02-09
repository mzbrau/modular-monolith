using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketSystem.Issue.Contracts;
using TicketSystem.Issue.Domain;

namespace TicketSystem.Issue.Application.Adapters;

internal class IssueModuleApiAdapter : IIssueModuleApi
{
    private readonly IssueService _issueService;

    public IssueModuleApiAdapter(IssueService issueService)
    {
        _issueService = issueService;
    }

    public async Task<CreateIssueResponse> CreateIssueAsync(CreateIssueRequest request)
    {
        var issueId = await _issueService.CreateIssueAsync(request.Title, request.Description, (IssuePriority)request.Priority, request.DueDate);
        return new CreateIssueResponse { IssueId = issueId };
    }

    public async Task<IssueDataContract?> GetIssueAsync(GetIssueRequest request)
    {
        try
        {
            var issue = await _issueService.GetIssueAsync(request.IssueId);
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

    public async Task UpdateIssueAsync(UpdateIssueRequest request)
    {
        await _issueService.UpdateIssueAsync(request.IssueId, request.Title, request.Description, (IssuePriority)request.Priority, request.DueDate);
    }

    public async Task AssignIssueToUserAsync(AssignIssueToUserRequest request)
    {
        await _issueService.AssignIssueToUserAsync(request.IssueId, request.UserId);
    }

    public async Task AssignIssueToTeamAsync(AssignIssueToTeamRequest request)
    {
        await _issueService.AssignIssueToTeamAsync(request.IssueId, request.TeamId);
    }

    public async Task UpdateIssueStatusAsync(UpdateIssueStatusRequest request)
    {
        await _issueService.UpdateIssueStatusAsync(request.IssueId, (IssueStatus)request.Status);
    }

    public async Task<List<IssueDataContract>> GetIssuesByUserAsync(GetIssuesByUserRequest request)
    {
        var issues = await _issueService.GetIssuesByUserAsync(request.UserId);
        return issues.Select(IssueConverter.ToDataContract).ToList();
    }

    public async Task<List<IssueDataContract>> GetIssuesByTeamAsync(GetIssuesByTeamRequest request)
    {
        var issues = await _issueService.GetIssuesByTeamAsync(request.TeamId);
        return issues.Select(IssueConverter.ToDataContract).ToList();
    }

    public async Task DeleteIssueAsync(DeleteIssueRequest request)
    {
        await _issueService.DeleteIssueAsync(request.IssueId);
    }
}
