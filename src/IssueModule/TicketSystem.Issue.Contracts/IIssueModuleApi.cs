namespace TicketSystem.Issue.Contracts;

public interface IIssueModuleApi
{
    Task<long> CreateIssueAsync(string title, string? description, int priority, DateTime? dueDate);
    Task<IssueDataContract?> GetIssueAsync(long issueId);
    Task<List<IssueDataContract>> GetAllIssuesAsync();
    Task UpdateIssueAsync(long issueId, string title, string? description, int priority, DateTime? dueDate);
    Task AssignIssueToUserAsync(long issueId, long? userId);
    Task AssignIssueToTeamAsync(long issueId, long? teamId);
    Task UpdateIssueStatusAsync(long issueId, int status);
    Task<List<IssueDataContract>> GetIssuesByUserAsync(long userId);
    Task<List<IssueDataContract>> GetIssuesByTeamAsync(long teamId);
    Task DeleteIssueAsync(long issueId);
}
