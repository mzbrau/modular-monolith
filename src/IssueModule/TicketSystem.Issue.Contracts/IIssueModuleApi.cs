namespace TicketSystem.Issue.Contracts;

public interface IIssueModuleApi
{
    Task<Guid> CreateIssueAsync(string title, string? description, int priority, DateTime? dueDate);
    Task<IssueDataContract?> GetIssueAsync(Guid issueId);
    Task<List<IssueDataContract>> GetAllIssuesAsync();
    Task UpdateIssueAsync(Guid issueId, string title, string? description, int priority, DateTime? dueDate);
    Task AssignIssueToUserAsync(Guid issueId, Guid? userId);
    Task AssignIssueToTeamAsync(Guid issueId, Guid? teamId);
    Task UpdateIssueStatusAsync(Guid issueId, int status);
    Task<List<IssueDataContract>> GetIssuesByUserAsync(Guid userId);
    Task<List<IssueDataContract>> GetIssuesByTeamAsync(Guid teamId);
    Task DeleteIssueAsync(Guid issueId);
}
