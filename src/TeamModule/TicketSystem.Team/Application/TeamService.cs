using TicketSystem.Team.Domain;
using TicketSystem.User.Contracts;

namespace TicketSystem.Team.Application;

internal class TeamService
{
    private readonly ITeamRepository _teamRepository;
    private readonly IUserModuleApi _userModuleApi;

    public TeamService(ITeamRepository teamRepository, IUserModuleApi userModuleApi)
    {
        _teamRepository = teamRepository;
        _userModuleApi = userModuleApi;
    }

    public async Task<TeamId> CreateTeamAsync(string name, string? description)
    {
        var teamId = await _teamRepository.GetNextIdAsync();
        var team = new TeamBusinessEntity(teamId, name, description);
        
        await _teamRepository.AddAsync(team);
        
        return teamId;
    }

    public async Task<TeamBusinessEntity> GetTeamAsync(TeamId teamId)
    {
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
            throw new KeyNotFoundException($"Team with ID '{teamId}' not found.");
        
        return team;
    }

    public async Task<IReadOnlyList<TeamBusinessEntity>> GetAllTeamsAsync()
    {
        return await _teamRepository.GetAllAsync();
    }

    public async Task UpdateTeamAsync(TeamId teamId, string name, string? description)
    {
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
            throw new KeyNotFoundException($"Team with ID '{teamId}' not found.");

        team.Update(name, description);
        await _teamRepository.UpdateAsync(team);
    }

    public async Task AddMemberToTeamAsync(TeamId teamId, long userId, TeamRole role = TeamRole.Member)
    {
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
            throw new KeyNotFoundException($"Team with ID '{teamId}' not found.");

        // Validate user exists via User module
        var userExists = await _userModuleApi.UserExistsAsync(userId);
        if (!userExists)
            throw new InvalidOperationException($"User with ID '{userId}' does not exist.");

        team.AddMember(userId, role);
        await _teamRepository.UpdateAsync(team);
    }

    public async Task RemoveMemberFromTeamAsync(TeamId teamId, long userId)
    {
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
            throw new KeyNotFoundException($"Team with ID '{teamId}' not found.");

        team.RemoveMember(userId);
        await _teamRepository.UpdateAsync(team);
    }

    public async Task<IReadOnlyList<TeamMemberBusinessEntity>> GetTeamMembersAsync(TeamId teamId)
    {
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
            throw new KeyNotFoundException($"Team with ID '{teamId}' not found.");

        return team.Members;
    }

    public async Task<bool> TeamExistsAsync(TeamId teamId)
    {
        return await _teamRepository.ExistsAsync(teamId);
    }

    public async Task<IReadOnlyList<long>> GetTeamMemberIdsAsync(TeamId teamId)
    {
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
            throw new KeyNotFoundException($"Team with ID '{teamId}' not found.");

        return team.Members.Select(m => m.UserId).ToList();
    }
}
