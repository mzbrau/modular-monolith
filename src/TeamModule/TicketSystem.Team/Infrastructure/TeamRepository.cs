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
        await _session.UpdateAsync(team);
        _logger.LogDebug("Team updated in database: {TeamId}", team.Id);
    }
}
