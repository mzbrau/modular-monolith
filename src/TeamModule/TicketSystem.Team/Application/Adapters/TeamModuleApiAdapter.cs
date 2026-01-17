using TicketSystem.Team.Application;
using TicketSystem.Team.Contracts;
using TicketSystem.Team.Domain;

namespace TicketSystem.Team.Application.Adapters;

internal class TeamModuleApiAdapter : ITeamModuleApi
{
    private readonly TeamService _teamService;

    public TeamModuleApiAdapter(TeamService teamService)
    {
        _teamService = teamService;
    }

    public async Task<CreateTeamResponse> CreateTeamAsync(CreateTeamRequest request)
    {
        var teamId = await _teamService.CreateTeamAsync(request.Name, request.Description);
        return new CreateTeamResponse { TeamId = teamId };
    }

    public async Task<TeamDataContract?> GetTeamAsync(GetTeamRequest request)
    {
        try
        {
            var team = await _teamService.GetTeamAsync(request.TeamId);
            return TeamConverter.ToDataContract(team);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    public async Task<List<TeamDataContract>> GetAllTeamsAsync()
    {
        var teams = await _teamService.GetAllTeamsAsync();
        return teams.Select(TeamConverter.ToDataContract).ToList();
    }

    public async Task UpdateTeamAsync(UpdateTeamRequest request)
    {
        await _teamService.UpdateTeamAsync(request.TeamId, request.Name, request.Description);
    }

    public async Task AddMemberToTeamAsync(AddMemberToTeamRequest request)
    {
        await _teamService.AddMemberToTeamAsync(request.TeamId, request.UserId, (TeamRole)request.Role);
    }

    public async Task RemoveMemberFromTeamAsync(RemoveMemberFromTeamRequest request)
    {
        await _teamService.RemoveMemberFromTeamAsync(request.TeamId, request.UserId);
    }

    public async Task<List<TeamMemberDataContract>> GetTeamMembersAsync(GetTeamMembersRequest request)
    {
        var members = await _teamService.GetTeamMembersAsync(request.TeamId);
        return members.Select(m => new TeamMemberDataContract
        {
            Id = m.Id,
            UserId = m.UserId,
            JoinedDate = m.JoinedDate,
            Role = (int)m.Role
        }).ToList();
    }

    public async Task<bool> TeamExistsAsync(TeamExistsRequest request)
    {
        return await _teamService.TeamExistsAsync(request.TeamId);
    }

    public async Task<List<long>> GetTeamMemberIdsAsync(GetTeamMemberIdsRequest request)
    {
        var memberIds = await _teamService.GetTeamMemberIdsAsync(request.TeamId);
        return memberIds.ToList();
    }
}
