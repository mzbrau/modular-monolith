using TicketSystem.Team.Application;
using TicketSystem.Team.Contracts;
using TicketSystem.Team.Domain;

namespace TicketSystem.Team.Adapters;

internal class TeamModuleApiAdapter : ITeamModuleApi
{
    private readonly TeamService _teamService;

    public TeamModuleApiAdapter(TeamService teamService)
    {
        _teamService = teamService;
    }

    public async Task<Guid> CreateTeamAsync(string name, string? description)
    {
        var teamId = await _teamService.CreateTeamAsync(name, description);
        return teamId.Value;
    }

    public async Task<TeamDataContract?> GetTeamAsync(Guid teamId)
    {
        try
        {
            var team = await _teamService.GetTeamAsync(new TeamId(teamId));
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

    public async Task UpdateTeamAsync(Guid teamId, string name, string? description)
    {
        await _teamService.UpdateTeamAsync(new TeamId(teamId), name, description);
    }

    public async Task AddMemberToTeamAsync(Guid teamId, Guid userId, int role)
    {
        await _teamService.AddMemberToTeamAsync(new TeamId(teamId), userId, (TeamRole)role);
    }

    public async Task RemoveMemberFromTeamAsync(Guid teamId, Guid userId)
    {
        await _teamService.RemoveMemberFromTeamAsync(new TeamId(teamId), userId);
    }

    public async Task<List<TeamMemberDataContract>> GetTeamMembersAsync(Guid teamId)
    {
        var members = await _teamService.GetTeamMembersAsync(new TeamId(teamId));
        return members.Select(m => new TeamMemberDataContract
        {
            Id = m.Id,
            UserId = m.UserId,
            JoinedDate = m.JoinedDate,
            Role = (int)m.Role
        }).ToList();
    }

    public async Task<bool> TeamExistsAsync(Guid teamId)
    {
        return await _teamService.TeamExistsAsync(new TeamId(teamId));
    }

    public async Task<List<Guid>> GetTeamMemberIdsAsync(Guid teamId)
    {
        var memberIds = await _teamService.GetTeamMemberIdsAsync(new TeamId(teamId));
        return memberIds.ToList();
    }
}
