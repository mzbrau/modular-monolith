namespace TicketSystem.Team.Contracts;

public interface ITeamModuleApi
{
    Task<long> CreateTeamAsync(string name, string? description);
    Task<TeamDataContract?> GetTeamAsync(long teamId);
    Task<List<TeamDataContract>> GetAllTeamsAsync();
    Task UpdateTeamAsync(long teamId, string name, string? description);
    Task AddMemberToTeamAsync(long teamId, long userId, int role);
    Task RemoveMemberFromTeamAsync(long teamId, long userId);
    Task<List<TeamMemberDataContract>> GetTeamMembersAsync(long teamId);
    Task<bool> TeamExistsAsync(long teamId);
    Task<List<long>> GetTeamMemberIdsAsync(long teamId);
}
