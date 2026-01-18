using System.Collections.Generic;
using System.Threading.Tasks;

namespace TicketSystem.Issue.Contracts;

public interface IIssueModuleApi
{
    Task<CreateIssueResponse> CreateIssueAsync(CreateIssueRequest request);
    Task<IssueDataContract?> GetIssueAsync(GetIssueRequest request);
    Task<List<IssueDataContract>> GetAllIssuesAsync();
    Task UpdateIssueAsync(UpdateIssueRequest request);
    Task AssignIssueToUserAsync(AssignIssueToUserRequest request);
    Task AssignIssueToTeamAsync(AssignIssueToTeamRequest request);
    Task UpdateIssueStatusAsync(UpdateIssueStatusRequest request);
    Task<List<IssueDataContract>> GetIssuesByUserAsync(GetIssuesByUserRequest request);
    Task<List<IssueDataContract>> GetIssuesByTeamAsync(GetIssuesByTeamRequest request);
    Task DeleteIssueAsync(DeleteIssueRequest request);
}
