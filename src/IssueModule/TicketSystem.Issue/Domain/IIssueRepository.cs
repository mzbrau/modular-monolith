namespace TicketSystem.Issue.Domain;

internal interface IIssueRepository
{
    Task<IssueBusinessEntity?> GetByIdAsync(IssueId id);
    Task<IReadOnlyList<IssueBusinessEntity>> GetAllAsync();
    Task<IReadOnlyList<IssueBusinessEntity>> GetByUserIdAsync(Guid userId);
    Task<IReadOnlyList<IssueBusinessEntity>> GetByTeamIdAsync(Guid teamId);
    Task<bool> ExistsAsync(IssueId id);
    Task AddAsync(IssueBusinessEntity issue);
    Task UpdateAsync(IssueBusinessEntity issue);
    Task DeleteAsync(IssueBusinessEntity issue);
}
