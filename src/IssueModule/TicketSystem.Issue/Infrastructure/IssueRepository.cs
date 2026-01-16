using NHibernate;
using NHibernate.Criterion;
using TicketSystem.Issue.Domain;

namespace TicketSystem.Issue.Infrastructure;

internal class IssueRepository : IIssueRepository
{
    private readonly ISession _session;

    public IssueRepository(ISession session)
    {
        _session = session;
    }

    public async Task<IssueBusinessEntity?> GetByIdAsync(IssueId id)
    {
        return await _session.GetAsync<IssueBusinessEntity>(id);
    }

    public async Task<IReadOnlyList<IssueBusinessEntity>> GetAllAsync()
    {
        var issues = await _session.QueryOver<IssueBusinessEntity>()
            .OrderBy(x => x.CreatedDate).Desc
            .ListAsync();
        return issues.ToList();
    }

    public async Task<IReadOnlyList<IssueBusinessEntity>> GetByUserIdAsync(Guid userId)
    {
        var issues = await _session.QueryOver<IssueBusinessEntity>()
            .Where(x => x.AssignedUserId == userId)
            .OrderBy(x => x.CreatedDate).Desc
            .ListAsync();
        return issues.ToList();
    }

    public async Task<IReadOnlyList<IssueBusinessEntity>> GetByTeamIdAsync(Guid teamId)
    {
        var issues = await _session.QueryOver<IssueBusinessEntity>()
            .Where(x => x.AssignedTeamId == teamId)
            .OrderBy(x => x.CreatedDate).Desc
            .ListAsync();
        return issues.ToList();
    }

    public async Task<bool> ExistsAsync(IssueId id)
    {
        var issue = await GetByIdAsync(id);
        return issue != null;
    }

    public async Task AddAsync(IssueBusinessEntity issue)
    {
        await _session.SaveAsync(issue);
    }

    public async Task UpdateAsync(IssueBusinessEntity issue)
    {
        await _session.UpdateAsync(issue);
    }

    public async Task DeleteAsync(IssueBusinessEntity issue)
    {
        await _session.DeleteAsync(issue);
    }
}
