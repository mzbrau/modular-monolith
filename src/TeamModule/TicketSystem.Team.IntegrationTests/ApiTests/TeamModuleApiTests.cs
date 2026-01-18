using TicketSystem.Testing.Common.Fixtures;
using TicketSystem.Team.Contracts;
using TicketSystem.Team.TestBuilders;
using TicketSystem.User.Contracts;
using TicketSystem.User.TestBuilders;

namespace TicketSystem.Team.IntegrationTests.ApiTests;

[TestFixture]
public class TeamModuleApiTests : ModuleTestBase
{
    private ITeamModuleApi _teamApi = null!;
    private IUserModuleApi _userApi = null!;
    private TeamTestDataSeeder _teams = null!;
    private UserTestDataSeeder _users = null!;

    [OneTimeSetUp]
    public override async Task TestClassSetUp()
    {
        await base.TestClassSetUp();
        
        _teamApi = GetService<ITeamModuleApi>();
        _userApi = GetService<IUserModuleApi>();
        _teams = new TeamTestDataSeeder(_teamApi);
        _users = new UserTestDataSeeder(_userApi);
    }

    [Test]
    public async Task CreateTeam_WithValidData_ReturnsTeamId()
    {
        // Arrange
        var request = _teams.ATeam()
            .WithName("Backend Team")
            .WithDescription("Handles backend services")
            .Build();

        // Act
        var response = await _teamApi.CreateTeamAsync(request);

        // Assert
        Assert.That(response.TeamId, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetTeam_AfterCreate_ReturnsCorrectData()
    {
        // Arrange
        var team = await _teams.ATeam()
            .WithName("Frontend Team")
            .WithDescription("Handles UI development")
            .CreateAndGetAsync();

        // Act
        var retrieved = await _teamApi.GetTeamAsync(
            new GetTeamRequest { TeamId = team.Id });

        // Assert
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.Name, Is.EqualTo("Frontend Team"));
        Assert.That(retrieved.Description, Is.EqualTo("Handles UI development"));
    }

    [Test]
    public async Task GetAllTeams_ReturnsAllCreatedTeams()
    {
        // Arrange
        var team1 = await _teams.ATeam()
            .WithName("Team Alpha")
            .CreateAndGetAsync();
        
        var team2 = await _teams.ATeam()
            .WithName("Team Beta")
            .CreateAndGetAsync();

        // Act
        var allTeams = await _teamApi.GetAllTeamsAsync();

        // Assert
        Assert.That(allTeams, Is.Not.Empty);
        Assert.That(allTeams.Any(t => t.Id == team1.Id), Is.True);
        Assert.That(allTeams.Any(t => t.Id == team2.Id), Is.True);
    }

    [Test]
    public async Task UpdateTeam_WithValidData_UpdatesTeamInformation()
    {
        // Arrange
        var team = await _teams.ATeam()
            .WithName("Original Team")
            .WithDescription("Original description")
            .CreateAndGetAsync();

        // Act
        await _teamApi.UpdateTeamAsync(new UpdateTeamRequest
        {
            TeamId = team.Id,
            Name = "Updated Team",
            Description = "Updated description"
        });

        // Assert
        var updated = await _teamApi.GetTeamAsync(new GetTeamRequest { TeamId = team.Id });
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Name, Is.EqualTo("Updated Team"));
        Assert.That(updated.Description, Is.EqualTo("Updated description"));
    }

    [Test]
    public async Task AddMemberToTeam_WithValidUser_AddsUserToTeam()
    {
        // Arrange
        var team = await _teams.CreateStandardTeamAsync();
        var user = await _users.CreateStandardUserAsync();

        // Act
        await _teamApi.AddMemberToTeamAsync(new AddMemberToTeamRequest
        {
            TeamId = team.Id,
            UserId = user.Id
        });

        // Assert
        var members = await _teamApi.GetTeamMembersAsync(
            new GetTeamMembersRequest { TeamId = team.Id });
        
        Assert.That(members, Has.Count.EqualTo(1));
        Assert.That(members[0].UserId, Is.EqualTo(user.Id));
    }

    [Test]
    public async Task RemoveMemberFromTeam_WithExistingMember_RemovesUserFromTeam()
    {
        // Arrange
        var user = await _users.CreateStandardUserAsync();
        var team = await _teams.ATeam()
            .WithMember(user.Id)
            .CreateAndGetAsync();

        // Act
        await _teamApi.RemoveMemberFromTeamAsync(new RemoveMemberFromTeamRequest
        {
            TeamId = team.Id,
            UserId = user.Id
        });

        // Assert
        var members = await _teamApi.GetTeamMembersAsync(
            new GetTeamMembersRequest { TeamId = team.Id });
        
        Assert.That(members, Is.Empty);
    }

    [Test]
    public async Task GetTeamMembers_WithMultipleMembers_ReturnsAllMembers()
    {
        // Arrange
        var user1 = await _users.AUser().WithName("Alice", "Anderson").CreateAndGetAsync();
        var user2 = await _users.AUser().WithName("Bob", "Brown").CreateAndGetAsync();
        var user3 = await _users.AUser().WithName("Charlie", "Chen").CreateAndGetAsync();
        
        var team = await _teams.ATeam()
            .WithMembers(user1.Id, user2.Id, user3.Id)
            .CreateAndGetAsync();

        // Act
        var members = await _teamApi.GetTeamMembersAsync(
            new GetTeamMembersRequest { TeamId = team.Id });

        // Assert
        Assert.That(members, Has.Count.EqualTo(3));
        Assert.That(members.Any(m => m.UserId == user1.Id), Is.True);
        Assert.That(members.Any(m => m.UserId == user2.Id), Is.True);
        Assert.That(members.Any(m => m.UserId == user3.Id), Is.True);
    }

    [Test]
    public async Task GetTeamMemberIds_ReturnsOnlyUserIds()
    {
        // Arrange
        var user1 = await _users.CreateStandardUserAsync();
        var user2 = await _users.CreateStandardUserAsync();
        
        var team = await _teams.ATeam()
            .WithMembers(user1.Id, user2.Id)
            .CreateAndGetAsync();

        // Act
        var memberIds = await _teamApi.GetTeamMemberIdsAsync(
            new GetTeamMemberIdsRequest { TeamId = team.Id });

        // Assert
        Assert.That(memberIds, Has.Count.EqualTo(2));
        Assert.That(memberIds, Does.Contain(user1.Id));
        Assert.That(memberIds, Does.Contain(user2.Id));
    }

    [Test]
    public async Task TeamExists_WithExistingTeam_ReturnsTrue()
    {
        // Arrange
        var team = await _teams.CreateStandardTeamAsync();

        // Act
        var exists = await _teamApi.TeamExistsAsync(new TeamExistsRequest { TeamId = team.Id });

        // Assert
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task TeamExists_WithNonExistingTeam_ReturnsFalse()
    {
        // Act
        var exists = await _teamApi.TeamExistsAsync(new TeamExistsRequest { TeamId = 999999 });

        // Assert
        Assert.That(exists, Is.False);
    }
}
