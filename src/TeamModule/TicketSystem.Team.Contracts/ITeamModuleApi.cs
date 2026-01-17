namespace TicketSystem.Team.Contracts;

public interface ITeamModuleApi
{
    Task<CreateTeamResponse> CreateTeamAsync(CreateTeamRequest request);
    Task<TeamDataContract?> GetTeamAsync(GetTeamRequest request);
    Task<List<TeamDataContract>> GetAllTeamsAsync();
    Task UpdateTeamAsync(UpdateTeamRequest request);
    Task AddMemberToTeamAsync(AddMemberToTeamRequest request);
    Task RemoveMemberFromTeamAsync(RemoveMemberFromTeamRequest request);
    Task<List<TeamMemberDataContract>> GetTeamMembersAsync(GetTeamMembersRequest request);
    Task<bool> TeamExistsAsync(TeamExistsRequest request);
    Task<List<long>> GetTeamMemberIdsAsync(GetTeamMemberIdsRequest request);
}
