using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NHibernate;
using TicketSystem.Team.Domain;

namespace TicketSystem.Team.Infrastructure;

internal class TeamRepository : ITeamRepository
{
    private readonly ISession _session;
    private readonly ILogger<TeamRepository> _logger;

    public TeamRepository(ISession session, ILogger<TeamRepository> logger)
    {
        _session = session;
        _logger = logger;
    }

    public async Task<TeamBusinessEntity?> GetByIdAsync(long id)
    {
        return await _session.GetAsync<TeamBusinessEntity>(id);
    }

    public async Task<IReadOnlyList<TeamBusinessEntity>> GetAllAsync()
    {
        var teams = await _session.QueryOver<TeamBusinessEntity>()
            .ListAsync();
        return teams.ToList();
    }

    public async Task<bool> ExistsAsync(long id)
    {
        var count = await _session.QueryOver<TeamBusinessEntity>()
            .Where(t => t.Id == id)
            .RowCountAsync();
        return count > 0;
    }

    public async Task AddAsync(TeamBusinessEntity team)
    {
        _logger.LogDebug("Saving new team to database: {TeamName}", team.Name);
        await _session.SaveAsync(team);
        _logger.LogDebug("Team saved successfully with ID: {TeamId}", team.Id);
    }

    public async Task UpdateAsync(TeamBusinessEntity team)
    {
        _logger.LogDebug("Updating team in database: {TeamId}, MemberCount: {MemberCount}", team.Id, team.Members.Count);
        // Only call Update for detached entities. If already tracked, updating can interfere
        // with NHibernate dirty-checking.
        if (!_session.Contains(team))
        {
            await _session.UpdateAsync(team);
        }

        // Some integration tests invoke module APIs directly (no HTTP transaction middleware),
        // so ensure changes are flushed to the database.
        // Only flush when there is no active transaction; otherwise the request transaction commit will flush.
        if (_session.Transaction is not { IsActive: true })
        {
            await _session.FlushAsync();
        }
        _logger.LogDebug("Team updated in database: {TeamId}", team.Id);
    }
}
