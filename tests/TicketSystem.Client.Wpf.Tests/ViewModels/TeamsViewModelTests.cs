using FluentAssertions;
using Moq;
using TicketSystem.Client.Wpf.Models;
using TicketSystem.Client.Wpf.Services;
using TicketSystem.Client.Wpf.ViewModels;
using TeamModel = TicketSystem.Client.Wpf.Models.Team;

namespace TicketSystem.Client.Wpf.Tests.ViewModels;

/// <summary>
/// Unit tests for TeamsViewModel class.
/// Requirements: 7.1, 8.2, 9.2, 9.4
/// </summary>
public class TeamsViewModelTests
{
    private readonly Mock<ITeamService> _mockTeamService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IDialogService> _mockDialogService;

    public TeamsViewModelTests()
    {
        _mockTeamService = new Mock<ITeamService>();
        _mockUserService = new Mock<IUserService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockDialogService.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
    }

    [Fact]
    public void Constructor_ShouldInitializeCollectionsAndCommands()
    {
        // Act
        var viewModel = new TeamsViewModel(
            _mockTeamService.Object,
            _mockUserService.Object,
            _mockDialogService.Object);

        // Assert
        viewModel.Teams.Should().NotBeNull();
        viewModel.Teams.Should().BeEmpty();
        viewModel.TeamMembers.Should().NotBeNull();
        viewModel.TeamMembers.Should().BeEmpty();
        viewModel.LoadTeamsCommand.Should().NotBeNull();
        viewModel.LoadTeamMembersCommand.Should().NotBeNull();
        viewModel.CreateTeamCommand.Should().NotBeNull();
        viewModel.EditTeamCommand.Should().NotBeNull();
        viewModel.AddMemberCommand.Should().NotBeNull();
        viewModel.RemoveMemberCommand.Should().NotBeNull();
    }

    [Fact]
    public async Task LoadTeamsCommand_ShouldPopulateTeamsCollection()
    {
        // Arrange
        var teams = new List<TeamModel>
        {
            new TeamModel { Id = "1", Name = "Team 1", Description = "Desc 1", CreatedDate = DateTime.UtcNow, Members = new List<TeamMember>() },
            new TeamModel { Id = "2", Name = "Team 2", Description = "Desc 2", CreatedDate = DateTime.UtcNow, Members = new List<TeamMember>() }
        };

        _mockTeamService
            .Setup(s => s.GetAllTeamsAsync())
            .ReturnsAsync(teams);

        var viewModel = new TeamsViewModel(
            _mockTeamService.Object,
            _mockUserService.Object,
            _mockDialogService.Object);

        // Act
        viewModel.LoadTeamsCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        viewModel.Teams.Should().HaveCount(2);
        viewModel.Teams[0].Name.Should().Be("Team 1");
        viewModel.Teams[1].Name.Should().Be("Team 2");
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task LoadTeamsCommand_OnError_ShouldSetErrorMessage()
    {
        // Arrange
        _mockTeamService
            .Setup(s => s.GetAllTeamsAsync())
            .ThrowsAsync(new Exception("Test error"));

        var viewModel = new TeamsViewModel(
            _mockTeamService.Object,
            _mockUserService.Object,
            _mockDialogService.Object);

        // Act
        viewModel.LoadTeamsCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        viewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        viewModel.ErrorMessage.Should().Contain("error");
    }

    [Fact]
    public async Task LoadTeamMembersCommand_ShouldPopulateTeamMembersCollection()
    {
        // Arrange
        var team = new TeamModel
        {
            Id = "team-1",
            Name = "Test Team",
            Description = "Test Description",
            CreatedDate = DateTime.UtcNow,
            Members = new List<TeamMember>()
        };

        var members = new List<TeamMember>
        {
            new TeamMember { Id = "1", UserId = "user-1", JoinedDate = DateTime.UtcNow, Role = 1 },
            new TeamMember { Id = "2", UserId = "user-2", JoinedDate = DateTime.UtcNow, Role = 2 }
        };

        _mockTeamService
            .Setup(s => s.GetTeamMembersAsync("team-1"))
            .ReturnsAsync(members);

        var viewModel = new TeamsViewModel(
            _mockTeamService.Object,
            _mockUserService.Object,
            _mockDialogService.Object);

        viewModel.Teams.Add(team);
        viewModel.SelectedTeam = team;

        // Act
        await Task.Delay(100); // Wait for automatic load triggered by SelectedTeam setter

        // Assert
        viewModel.TeamMembers.Should().HaveCount(2);
        viewModel.TeamMembers[0].UserId.Should().Be("user-1");
        viewModel.TeamMembers[1].UserId.Should().Be("user-2");
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task CreateTeamCommand_WithValidName_ShouldCallServiceAndRefreshList()
    {
        // Arrange
        _mockTeamService
            .Setup(s => s.CreateTeamAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("new-team-id");

        _mockTeamService
            .Setup(s => s.GetAllTeamsAsync())
            .ReturnsAsync(new List<TeamModel>
            {
                new TeamModel { Id = "new-team-id", Name = "New Team", Description = "New Desc", CreatedDate = DateTime.UtcNow, Members = new List<TeamMember>() }
            });

        var viewModel = new TeamsViewModel(
            _mockTeamService.Object,
            _mockUserService.Object,
            _mockDialogService.Object);

        viewModel.TeamName = "New Team";
        viewModel.TeamDescription = "New Desc";

        // Act
        viewModel.CreateTeamCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        _mockTeamService.Verify(s => s.CreateTeamAsync("New Team", "New Desc"), Times.Once);
        _mockTeamService.Verify(s => s.GetAllTeamsAsync(), Times.Once);
        viewModel.Teams.Should().HaveCount(1);
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task CreateTeamCommand_WithEmptyName_ShouldNotCallServiceAndSetErrorMessage()
    {
        // Arrange
        var viewModel = new TeamsViewModel(
            _mockTeamService.Object,
            _mockUserService.Object,
            _mockDialogService.Object);

        viewModel.TeamName = "";
        viewModel.TeamDescription = "Test Description";

        // Act
        viewModel.CreateTeamCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        _mockTeamService.Verify(s => s.CreateTeamAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        viewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        viewModel.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public async Task EditTeamCommand_WithValidName_ShouldCallServiceAndRefreshTeam()
    {
        // Arrange
        var team = new TeamModel
        {
            Id = "test-id",
            Name = "Original Team",
            Description = "Original Description",
            CreatedDate = DateTime.UtcNow,
            Members = new List<TeamMember>()
        };

        _mockTeamService
            .Setup(s => s.UpdateTeamAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _mockTeamService
            .Setup(s => s.GetTeamAsync("test-id"))
            .ReturnsAsync(new TeamModel
            {
                Id = "test-id",
                Name = "Updated Team",
                Description = "Updated Description",
                CreatedDate = team.CreatedDate,
                Members = new List<TeamMember>()
            });

        var viewModel = new TeamsViewModel(
            _mockTeamService.Object,
            _mockUserService.Object,
            _mockDialogService.Object);

        viewModel.Teams.Add(team);
        viewModel.SelectedTeam = team;
        viewModel.TeamName = "Updated Team";
        viewModel.TeamDescription = "Updated Description";

        // Act
        viewModel.EditTeamCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        _mockTeamService.Verify(s => s.UpdateTeamAsync("test-id", "Updated Team", "Updated Description"), Times.Once);
        _mockTeamService.Verify(s => s.GetTeamAsync("test-id"), Times.Once);
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task AddMemberCommand_ShouldCallServiceAndRefreshMembers()
    {
        // Arrange
        var team = new TeamModel
        {
            Id = "team-1",
            Name = "Test Team",
            Description = "Test Description",
            CreatedDate = DateTime.UtcNow,
            Members = new List<TeamMember>()
        };

        _mockTeamService
            .Setup(s => s.AddMemberToTeamAsync("team-1", "user-123", 1))
            .Returns(Task.CompletedTask);

        _mockTeamService
            .Setup(s => s.GetTeamMembersAsync("team-1"))
            .ReturnsAsync(new List<TeamMember>
            {
                new TeamMember { Id = "1", UserId = "user-123", JoinedDate = DateTime.UtcNow, Role = 1 }
            });

        var viewModel = new TeamsViewModel(
            _mockTeamService.Object,
            _mockUserService.Object,
            _mockDialogService.Object);

        viewModel.Teams.Add(team);
        viewModel.SelectedTeam = team;
        viewModel.SelectedUserId = "user-123";
        viewModel.SelectedRole = 1;

        // Wait for initial member load
        await Task.Delay(100);

        // Act
        viewModel.AddMemberCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        _mockTeamService.Verify(s => s.AddMemberToTeamAsync("team-1", "user-123", 1), Times.Once);
        _mockTeamService.Verify(s => s.GetTeamMembersAsync("team-1"), Times.AtLeastOnce);
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task RemoveMemberCommand_ShouldCallServiceAndRefreshMembers()
    {
        // Arrange
        var team = new TeamModel
        {
            Id = "team-1",
            Name = "Test Team",
            Description = "Test Description",
            CreatedDate = DateTime.UtcNow,
            Members = new List<TeamMember>()
        };

        var member = new TeamMember { Id = "1", UserId = "user-123", JoinedDate = DateTime.UtcNow, Role = 1 };

        _mockTeamService
            .Setup(s => s.RemoveMemberFromTeamAsync("team-1", "user-123"))
            .Returns(Task.CompletedTask);

        _mockTeamService
            .Setup(s => s.GetTeamMembersAsync("team-1"))
            .ReturnsAsync(new List<TeamMember>());

        var viewModel = new TeamsViewModel(
            _mockTeamService.Object,
            _mockUserService.Object,
            _mockDialogService.Object);

        viewModel.Teams.Add(team);
        viewModel.SelectedTeam = team;
        viewModel.TeamMembers.Add(member);

        // Wait for initial member load
        await Task.Delay(100);

        // Act
        viewModel.RemoveMemberCommand.Execute("user-123");
        await Task.Delay(100); // Wait for async operation

        // Assert
        _mockTeamService.Verify(s => s.RemoveMemberFromTeamAsync("team-1", "user-123"), Times.Once);
        _mockTeamService.Verify(s => s.GetTeamMembersAsync("team-1"), Times.AtLeastOnce);
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public void SelectedTeam_WhenSet_ShouldUpdateFormFields()
    {
        // Arrange
        var team = new TeamModel
        {
            Id = "test-id",
            Name = "Test Team",
            Description = "Test Description",
            CreatedDate = DateTime.UtcNow,
            Members = new List<TeamMember>()
        };

        _mockTeamService
            .Setup(s => s.GetTeamMembersAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<TeamMember>());

        var viewModel = new TeamsViewModel(
            _mockTeamService.Object,
            _mockUserService.Object,
            _mockDialogService.Object);

        // Act
        viewModel.SelectedTeam = team;

        // Assert
        viewModel.TeamName.Should().Be("Test Team");
        viewModel.TeamDescription.Should().Be("Test Description");
    }

    [Fact]
    public void SelectedTeam_WhenSetToNull_ShouldClearTeamMembers()
    {
        // Arrange
        var team = new TeamModel
        {
            Id = "test-id",
            Name = "Test Team",
            Description = "Test Description",
            CreatedDate = DateTime.UtcNow,
            Members = new List<TeamMember>()
        };

        _mockTeamService
            .Setup(s => s.GetTeamMembersAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<TeamMember>
            {
                new TeamMember { Id = "1", UserId = "user-1", JoinedDate = DateTime.UtcNow, Role = 1 }
            });

        var viewModel = new TeamsViewModel(
            _mockTeamService.Object,
            _mockUserService.Object,
            _mockDialogService.Object);

        viewModel.SelectedTeam = team;

        // Act
        viewModel.SelectedTeam = null;

        // Assert
        viewModel.TeamMembers.Should().BeEmpty();
    }
}
