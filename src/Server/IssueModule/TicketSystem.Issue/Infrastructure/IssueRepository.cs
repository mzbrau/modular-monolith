using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NHibernate;
using TicketSystem.Issue.Domain;

namespace TicketSystem.Issue.Infrastructure;

internal class IssueRepository : IIssueRepository
{
    private readonly ISession _session;
    private readonly ILogger<IssueRepository> _logger;

    public IssueRepository(ISession session, ILogger<IssueRepository> logger)
    {
        _session = session;
        _logger = logger;
    }

    public async Task<IssueBusinessEntity?> GetByIdAsync(long id)
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

    public async Task<IReadOnlyList<IssueBusinessEntity>> GetByUserIdAsync(long userId)
    {
        _logger.LogDebug("Fetching issues for user: {UserId}", userId);
        var issues = await _session.QueryOver<IssueBusinessEntity>()
            .Where(x => x.AssignedUserId == userId)
            .OrderBy(x => x.CreatedDate).Desc
            .ListAsync();
        _logger.LogDebug("Found {Count} issues for user: {UserId}", issues.Count, userId);
        return issues.ToList();
    }

    public async Task<IReadOnlyList<IssueBusinessEntity>> GetByTeamIdAsync(long teamId)
    {
        _logger.LogDebug("Fetching issues for team: {TeamId}", teamId);
        var issues = await _session.QueryOver<IssueBusinessEntity>()
            .Where(x => x.AssignedTeamId == teamId)
            .OrderBy(x => x.CreatedDate).Desc
            .ListAsync();
        _logger.LogDebug("Found {Count} issues for team: {TeamId}", issues.Count, teamId);
        return issues.ToList();
    }

    public async Task<bool> ExistsAsync(long id)
    {
        var issue = await GetByIdAsync(id);
        return issue != null;
    }

    public async Task AddAsync(IssueBusinessEntity issue)
    {
        _logger.LogDebug("Saving new issue to database: {Title}, Priority: {Priority}", issue.Title, issue.Priority);
        await _session.SaveAsync(issue);
        _logger.LogDebug("Issue saved successfully with ID: {IssueId}", issue.Id);
    }

    public async Task UpdateAsync(IssueBusinessEntity issue)
    {
        // Only call Update for detached entities. If the entity is already tracked by the
        // current session, calling Update can reset the dirty-check snapshot and prevent
        // scalar changes (e.g. AssignedUserId) from being persisted.
        if (!_session.Contains(issue))
        {
            await _session.UpdateAsync(issue);
        }

        // Integration tests call module APIs directly (bypassing the HTTP transaction middleware),
        // so we can't rely on a request transaction commit to flush updates.
        // Only flush when there's no active transaction; otherwise the middleware commit will flush.
        if (_session.Transaction is not { IsActive: true })
        {
            await _session.FlushAsync();
        }
    }

    public async Task DeleteAsync(IssueBusinessEntity issue)
    {
        _logger.LogDebug("Deleting issue from database: {IssueId}", issue.Id);
        await _session.DeleteAsync(issue);
        _logger.LogDebug("Issue deleted from database: {IssueId}", issue.Id);
    }
}
