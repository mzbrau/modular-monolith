namespace TicketSystem.Team.Domain;

internal interface ITeamRepository
{
    Task<TeamId> GetNextIdAsync();
    Task<TeamBusinessEntity?> GetByIdAsync(TeamId id);
    Task<IReadOnlyList<TeamBusinessEntity>> GetAllAsync();
    Task<bool> ExistsAsync(TeamId id);
    Task AddAsync(TeamBusinessEntity team);
    Task UpdateAsync(TeamBusinessEntity team);
}
