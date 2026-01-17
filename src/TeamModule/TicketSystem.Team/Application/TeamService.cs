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

    public async Task<long> CreateTeamAsync(string name, string? description)
    {
        var team = new TeamBusinessEntity(0, name, description);
        
        await _teamRepository.AddAsync(team);
        
        return team.Id;
    }

    public async Task<TeamBusinessEntity> GetTeamAsync(long teamId)
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

    public async Task UpdateTeamAsync(long teamId, string name, string? description)
    {
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
            throw new KeyNotFoundException($"Team with ID '{teamId}' not found.");

        team.Update(name, description);
        await _teamRepository.UpdateAsync(team);
    }

    public async Task AddMemberToTeamAsync(long teamId, long userId, TeamRole role = TeamRole.Member)
    {
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
            throw new KeyNotFoundException($"Team with ID '{teamId}' not found.");

        // Validate user exists via User module
        var userExists = await _userModuleApi.UserExistsAsync(new UserExistsRequest { UserId = userId });
        if (!userExists)
            throw new InvalidOperationException($"User with ID '{userId}' does not exist.");

        team.AddMember(userId, role);
        await _teamRepository.UpdateAsync(team);
    }

    public async Task RemoveMemberFromTeamAsync(long teamId, long userId)
    {
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
            throw new KeyNotFoundException($"Team with ID '{teamId}' not found.");

        team.RemoveMember(userId);
        await _teamRepository.UpdateAsync(team);
    }

    public async Task<IReadOnlyList<TeamMemberBusinessEntity>> GetTeamMembersAsync(long teamId)
    {
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
            throw new KeyNotFoundException($"Team with ID '{teamId}' not found.");

        return team.Members;
    }

    public async Task<bool> TeamExistsAsync(long teamId)
    {
        return await _teamRepository.ExistsAsync(teamId);
    }

    public async Task<IReadOnlyList<long>> GetTeamMemberIdsAsync(long teamId)
    {
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
            throw new KeyNotFoundException($"Team with ID '{teamId}' not found.");

        return team.Members.Select(m => m.UserId).ToList();
    }
}
