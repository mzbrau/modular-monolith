namespace TicketSystem.Issue.Domain;

internal interface IIssueRepository
{
    Task<IssueBusinessEntity?> GetByIdAsync(long id);
    Task<IReadOnlyList<IssueBusinessEntity>> GetAllAsync();
    Task<IReadOnlyList<IssueBusinessEntity>> GetByUserIdAsync(long userId);
    Task<IReadOnlyList<IssueBusinessEntity>> GetByTeamIdAsync(long teamId);
    Task<bool> ExistsAsync(long id);
    Task AddAsync(IssueBusinessEntity issue);
    Task UpdateAsync(IssueBusinessEntity issue);
    Task DeleteAsync(IssueBusinessEntity issue);
}
