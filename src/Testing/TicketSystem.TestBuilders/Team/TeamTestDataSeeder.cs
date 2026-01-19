using TicketSystem.Team.Contracts;

namespace TicketSystem.TestBuilders;

/// <summary>
/// Factory class for creating TeamBuilders within a test context.
/// </summary>
public class TeamTestDataSeeder
{
    private readonly ITeamModuleApi _teamModuleApi;

    public TeamTestDataSeeder(ITeamModuleApi teamModuleApi)
    {
        _teamModuleApi = teamModuleApi;
    }

    public TeamBuilder ATeam() => new TeamBuilder(_teamModuleApi);

    public TeamBuilder ADevelopmentTeam() => new TeamBuilder(_teamModuleApi)
        .WithName("Development Team")
        .WithDescription("A development team");

    /// <summary>
    /// Creates a standard test team with default values.
    /// </summary>
    public async Task<TeamDataContract> CreateStandardTeamAsync()
    {
        return await ATeam().CreateAndGetAsync();
    }
}
