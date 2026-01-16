namespace TicketSystem.Issue.Domain;

internal interface IIssueRepository
{
    Task<IssueId> GetNextIdAsync();
    Task<IssueBusinessEntity?> GetByIdAsync(IssueId id);
    Task<IReadOnlyList<IssueBusinessEntity>> GetAllAsync();
    Task<IReadOnlyList<IssueBusinessEntity>> GetByUserIdAsync(long userId);
    Task<IReadOnlyList<IssueBusinessEntity>> GetByTeamIdAsync(long teamId);
    Task<bool> ExistsAsync(IssueId id);
    Task AddAsync(IssueBusinessEntity issue);
    Task UpdateAsync(IssueBusinessEntity issue);
    Task DeleteAsync(IssueBusinessEntity issue);
}
