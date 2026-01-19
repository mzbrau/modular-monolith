using TicketSystem.Testing.Common.Builders;
using TicketSystem.Team.Contracts;

namespace TicketSystem.TestBuilders;

public class TeamBuilder : BuilderBase<TeamBuilder, CreateTeamRequest>
{
    private readonly ITeamModuleApi _teamModuleApi;
    
    private string _name = $"Test Team {Guid.NewGuid().ToString("N")[..8]}";
    private string _description = "A test team";
    private readonly List<long> _memberUserIds = new();
    private long? _createdTeamId;

    public TeamBuilder(ITeamModuleApi teamModuleApi)
    {
        _teamModuleApi = teamModuleApi;
    }
    
    public long CreatedTeamId => _createdTeamId ?? throw new InvalidOperationException("Team has not been created yet. Call CreateAsync first.");

    public TeamBuilder WithName(string name)
    {
        _name = name;
        return This;
    }

    public TeamBuilder WithDescription(string description)
    {
        _description = description;
        return This;
    }

    public TeamBuilder WithMember(long userId)
    {
        _memberUserIds.Add(userId);
        return This;
    }

    public TeamBuilder WithMembers(params long[] userIds)
    {
        _memberUserIds.AddRange(userIds);
        return This;
    }

    public override CreateTeamRequest Build()
    {
        return new CreateTeamRequest
        {
            Name = _name,
            Description = _description
        };
    }

    public override async Task<CreateTeamRequest> CreateAsync()
    {
        var request = Build();
        var response = await _teamModuleApi.CreateTeamAsync(request);
        _createdTeamId = response.TeamId;
        
        // Add members if specified
        foreach (var userId in _memberUserIds)
        {
            await _teamModuleApi.AddMemberToTeamAsync(new AddMemberToTeamRequest
            {
                TeamId = response.TeamId,
                UserId = userId
            });
        }
        
        return request;
    }

    public async Task<TeamDataContract> CreateAndGetAsync()
    {
        await CreateAsync();
        var team = await _teamModuleApi.GetTeamAsync(new GetTeamRequest { TeamId = CreatedTeamId });
        return team ?? throw new InvalidOperationException("Team was created but could not be retrieved");
    }
}
