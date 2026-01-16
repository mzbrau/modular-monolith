using NSubstitute;
using TicketSystem.Team.Application;
using TicketSystem.Team.Domain;
using TicketSystem.User.Contracts;

namespace TicketSystem.TeamModule.Tests;

[TestFixture]
public class TeamServiceTests
{
    private ITeamRepository _teamRepository = null!;
    private IUserModuleApi _userModuleApi = null!;
    private TeamService _teamService = null!;

    [SetUp]
    public void Setup()
    {
        _teamRepository = Substitute.For<ITeamRepository>();
        _userModuleApi = Substitute.For<IUserModuleApi>();
        _teamService = new TeamService(_teamRepository, _userModuleApi);
    }

    [Test]
    public async Task CreateTeamAsync_WithValidData_CreatesTeam()
    {
        // Arrange
        const string name = "Development Team";
        const string description = "Main development team";
        
        _teamRepository.AddAsync(Arg.Any<TeamBusinessEntity>()).Returns(Task.CompletedTask);

        // Act
        var teamId = await _teamService.CreateTeamAsync(name, description);

        // Assert
        Assert.That(teamId.Value, Is.Not.EqualTo(Guid.Empty));
        await _teamRepository.Received(1).AddAsync(Arg.Is<TeamBusinessEntity>(t => 
            t.Name == name && t.Description == description));
    }

    [Test]
    public async Task GetTeamAsync_WithExistingTeam_ReturnsTeam()
    {
        // Arrange
        var teamId = TeamId.New();
        var team = new TeamBusinessEntity(teamId, "Dev Team", "Description");
        
        _teamRepository.GetByIdAsync(teamId).Returns(team);

        // Act
        var result = await _teamService.GetTeamAsync(teamId);

        // Assert
        Assert.That(result, Is.EqualTo(team));
    }

    [Test]
    public async Task GetTeamAsync_WithNonExistingTeam_ThrowsKeyNotFoundException()
    {
        // Arrange
        var teamId = TeamId.New();
        _teamRepository.GetByIdAsync(teamId).Returns((TeamBusinessEntity?)null);

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () => 
            await _teamService.GetTeamAsync(teamId));
    }

    [Test]
    public async Task AddMemberToTeamAsync_WithValidUserAndTeam_AddsMember()
    {
        // Arrange
        var teamId = TeamId.New();
        var userId = Guid.NewGuid();
        var team = new TeamBusinessEntity(teamId, "Dev Team", "Description");
        
        _teamRepository.GetByIdAsync(teamId).Returns(team);
        _userModuleApi.UserExistsAsync(userId).Returns(true);
        _teamRepository.UpdateAsync(team).Returns(Task.CompletedTask);

        // Act
        await _teamService.AddMemberToTeamAsync(teamId, userId, TeamRole.Member);

        // Assert
        Assert.That(team.Members.Any(m => m.UserId == userId), Is.True);
        await _teamRepository.Received(1).UpdateAsync(team);
    }

    [Test]
    public async Task AddMemberToTeamAsync_WithNonExistingUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var teamId = TeamId.New();
        var userId = Guid.NewGuid();
        var team = new TeamBusinessEntity(teamId, "Dev Team", "Description");
        
        _teamRepository.GetByIdAsync(teamId).Returns(team);
        _userModuleApi.UserExistsAsync(userId).Returns(false);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await _teamService.AddMemberToTeamAsync(teamId, userId));
    }

    [Test]
    public async Task RemoveMemberFromTeamAsync_WithExistingMember_RemovesMember()
    {
        // Arrange
        var teamId = TeamId.New();
        var userId = Guid.NewGuid();
        var team = new TeamBusinessEntity(teamId, "Dev Team", "Description");
        team.AddMember(userId, TeamRole.Member);
        
        _teamRepository.GetByIdAsync(teamId).Returns(team);
        _teamRepository.UpdateAsync(team).Returns(Task.CompletedTask);

        // Act
        await _teamService.RemoveMemberFromTeamAsync(teamId, userId);

        // Assert
        Assert.That(team.Members.Any(m => m.UserId == userId), Is.False);
    }
}
