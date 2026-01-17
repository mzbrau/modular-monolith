namespace TicketSystem.Team.Domain;

internal interface ITeamRepository
{
    Task<TeamBusinessEntity?> GetByIdAsync(long id);
    Task<IReadOnlyList<TeamBusinessEntity>> GetAllAsync();
    Task<bool> ExistsAsync(long id);
    Task AddAsync(TeamBusinessEntity team);
    Task UpdateAsync(TeamBusinessEntity team);
}
