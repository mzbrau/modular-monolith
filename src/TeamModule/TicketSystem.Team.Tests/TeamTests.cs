using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using TicketSystem.Team.Application;
using TicketSystem.Team.Configuration;
using TicketSystem.Team.Domain;
using TicketSystem.User.Contracts;

namespace TicketSystem.TeamModule.Tests;

[TestFixture]
public class TeamServiceTests
{
    private Mock<ITeamRepository> _teamRepository = null!;
    private Mock<IUserModuleApi> _userModuleApi = null!;
    private Mock<ILogger<TeamService>> _logger = null!;
    private Mock<IOptionsMonitor<TeamSettings>> _settings = null!;
    private TeamService _teamService = null!;

    [SetUp]
    public void Setup()
    {
        _teamRepository = new Mock<ITeamRepository>();
        _userModuleApi = new Mock<IUserModuleApi>();
        _logger = new Mock<ILogger<TeamService>>();
        _settings = new Mock<IOptionsMonitor<TeamSettings>>();
        
        // Setup default settings
        var defaultSettings = new TeamSettings();
        _settings.Setup(s => s.CurrentValue).Returns(defaultSettings);
        
        _teamService = new TeamService(_teamRepository.Object, _userModuleApi.Object, _logger.Object, _settings.Object);
    }

    [Test]
    public async Task CreateTeamAsync_WithValidData_CreatesTeam()
    {
        // Arrange
        const string name = "Development Team";
        const string description = "Main development team";
        
        _teamRepository.Setup(x => x.AddAsync(It.IsAny<TeamBusinessEntity>())).Returns(Task.CompletedTask);

        // Act
        var teamId = await _teamService.CreateTeamAsync(name, description);

        // Assert
        Assert.That(teamId, Is.EqualTo(0)); // ID is 0 before saving
        _teamRepository.Verify(x => x.AddAsync(It.Is<TeamBusinessEntity>(t => 
            t.Name == name && t.Description == description)), Times.Once);
    }

    [Test]
    public async Task GetTeamAsync_WithExistingTeam_ReturnsTeam()
    {
        // Arrange
        const long teamId = 1;
        var team = new TeamBusinessEntity(teamId, "Dev Team", "Description");
        
        _teamRepository.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);

        // Act
        var result = await _teamService.GetTeamAsync(teamId);

        // Assert
        Assert.That(result, Is.EqualTo(team));
    }

    [Test]
    public async Task GetTeamAsync_WithNonExistingTeam_ThrowsKeyNotFoundException()
    {
        // Arrange
        const long teamId = 1;
        _teamRepository.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync((TeamBusinessEntity?)null);

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () => 
            await _teamService.GetTeamAsync(teamId));
    }

    [Test]
    public async Task AddMemberToTeamAsync_WithValidUserAndTeam_AddsMember()
    {
        // Arrange
        const long teamId = 1;
        long userId = 1;
        var team = new TeamBusinessEntity(teamId, "Dev Team", "Description");
        
        _teamRepository.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);
        _userModuleApi.Setup(x => x.UserExistsAsync(It.Is<UserExistsRequest>(r => r.UserId == userId))).ReturnsAsync(true);
        _teamRepository.Setup(x => x.UpdateAsync(team)).Returns(Task.CompletedTask);

        // Act
        await _teamService.AddMemberToTeamAsync(teamId, userId, TeamRole.Member);

        // Assert
        Assert.That(team.Members.Any(m => m.UserId == userId), Is.True);
        _teamRepository.Verify(x => x.UpdateAsync(team), Times.Once);
    }

    [Test]
    public async Task AddMemberToTeamAsync_WithNonExistingUser_ThrowsInvalidOperationException()
    {
        // Arrange
        const long teamId = 1;
        long userId = 1;
        var team = new TeamBusinessEntity(teamId, "Dev Team", "Description");
        
        _teamRepository.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);
        _userModuleApi.Setup(x => x.UserExistsAsync(It.Is<UserExistsRequest>(r => r.UserId == userId))).ReturnsAsync(false);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await _teamService.AddMemberToTeamAsync(teamId, userId));
    }

    [Test]
    public async Task RemoveMemberFromTeamAsync_WithExistingMember_RemovesMember()
    {
        // Arrange
        const long teamId = 1;
        long userId = 1;
        var team = new TeamBusinessEntity(teamId, "Dev Team", "Description");
        team.AddMember(userId, TeamRole.Member);
        
        _teamRepository.Setup(x => x.GetByIdAsync(teamId)).ReturnsAsync(team);
        _teamRepository.Setup(x => x.UpdateAsync(team)).Returns(Task.CompletedTask);

        // Act
        await _teamService.RemoveMemberFromTeamAsync(teamId, userId);

        // Assert
        Assert.That(team.Members.Any(m => m.UserId == userId), Is.False);
    }
}
