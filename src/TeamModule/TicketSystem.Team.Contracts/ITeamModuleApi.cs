namespace TicketSystem.Team.Contracts;

public interface ITeamModuleApi
{
    Task<Guid> CreateTeamAsync(string name, string? description);
    Task<TeamDataContract?> GetTeamAsync(Guid teamId);
    Task<List<TeamDataContract>> GetAllTeamsAsync();
    Task UpdateTeamAsync(Guid teamId, string name, string? description);
    Task AddMemberToTeamAsync(Guid teamId, Guid userId, int role);
    Task RemoveMemberFromTeamAsync(Guid teamId, Guid userId);
    Task<List<TeamMemberDataContract>> GetTeamMembersAsync(Guid teamId);
    Task<bool> TeamExistsAsync(Guid teamId);
    Task<List<Guid>> GetTeamMemberIdsAsync(Guid teamId);
}
