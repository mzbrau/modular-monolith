using NHibernate;
using TicketSystem.Team.Domain;

namespace TicketSystem.Team.Infrastructure;

internal class TeamRepository : ITeamRepository
{
    private readonly ISession _session;

    public TeamRepository(ISession session)
    {
        _session = session;
    }

    public async Task<TeamBusinessEntity?> GetByIdAsync(TeamId id)
    {
        return await _session.GetAsync<TeamBusinessEntity>(id);
    }

    public async Task<IReadOnlyList<TeamBusinessEntity>> GetAllAsync()
    {
        var teams = await _session.QueryOver<TeamBusinessEntity>()
            .ListAsync();
        return teams.ToList();
    }

    public async Task<bool> ExistsAsync(TeamId id)
    {
        var count = await _session.QueryOver<TeamBusinessEntity>()
            .Where(t => t.Id == id)
            .RowCountAsync();
        return count > 0;
    }

    public async Task AddAsync(TeamBusinessEntity team)
    {
        await _session.SaveAsync(team);
    }

    public async Task UpdateAsync(TeamBusinessEntity team)
    {
        await _session.UpdateAsync(team);
    }
}
