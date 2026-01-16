using NSubstitute;
using TicketSystem.Issue.Application;
using TicketSystem.Issue.Domain;
using TicketSystem.Team.Contracts;
using TicketSystem.User.Contracts;

namespace TicketSystem.Issue.Tests;

[TestFixture]
public class IssueServiceTests
{
    private IIssueRepository _issueRepository = null!;
    private IUserModuleApi _userModuleApi = null!;
    private ITeamModuleApi _teamModuleApi = null!;
    private IssueService _issueService = null!;

    [SetUp]
    public void Setup()
    {
        _issueRepository = Substitute.For<IIssueRepository>();
        _userModuleApi = Substitute.For<IUserModuleApi>();
        _teamModuleApi = Substitute.For<ITeamModuleApi>();
        _issueService = new IssueService(_issueRepository, _userModuleApi, _teamModuleApi);
    }

    [Test]
    public async Task CreateIssueAsync_WithValidData_CreatesIssue()
    {
        // Arrange
        const string title = "Bug in login";
        const string description = "Users cannot login";
        var priority = IssuePriority.High;
        DateTime? dueDate = DateTime.UtcNow.AddDays(7);
        
        _issueRepository.AddAsync(Arg.Any<IssueBusinessEntity>()).Returns(Task.CompletedTask);

        // Act
        var issueId = await _issueService.CreateIssueAsync(title, description, priority, dueDate);

        // Assert
        Assert.That(issueId.Value, Is.Not.EqualTo(Guid.Empty));
        await _issueRepository.Received(1).AddAsync(Arg.Is<IssueBusinessEntity>(t => 
            t.Title == title && t.Description == description && t.Priority == priority));
    }

    [Test]
    public async Task GetIssueAsync_WithExistingIssue_ReturnsIssue()
    {
        // Arrange
        var issueId = IssueId.New();
        var issue = new IssueBusinessEntity(issueId, "Bug", "Description", IssuePriority.Medium, null);
        
        _issueRepository.GetByIdAsync(issueId).Returns(issue);

        // Act
        var result = await _issueService.GetIssueAsync(issueId);

        // Assert
        Assert.That(result, Is.EqualTo(issue));
    }

    [Test]
    public async Task GetIssueAsync_WithNonExistingIssue_ThrowsKeyNotFoundException()
    {
        // Arrange
        var issueId = IssueId.New();
        _issueRepository.GetByIdAsync(issueId).Returns((IssueBusinessEntity?)null);

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () => 
            await _issueService.GetIssueAsync(issueId));
    }

    [Test]
    public async Task AssignIssueToUserAsync_WithValidUser_AssignsIssue()
    {
        // Arrange
        var issueId = IssueId.New();
        var userId = Guid.NewGuid();
        var issue = new IssueBusinessEntity(issueId, "Bug", "Description", IssuePriority.Medium, null);
        
        _issueRepository.GetByIdAsync(issueId).Returns(issue);
        _userModuleApi.UserExistsAsync(userId).Returns(true);
        _issueRepository.UpdateAsync(issue).Returns(Task.CompletedTask);

        // Act
        await _issueService.AssignIssueToUserAsync(issueId, userId);

        // Assert
        Assert.That(issue.AssignedUserId, Is.EqualTo(userId));
        await _issueRepository.Received(1).UpdateAsync(issue);
    }

    [Test]
    public async Task AssignIssueToUserAsync_WithNonExistingUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var issueId = IssueId.New();
        var userId = Guid.NewGuid();
        var issue = new IssueBusinessEntity(issueId, "Bug", "Description", IssuePriority.Medium, null);
        
        _issueRepository.GetByIdAsync(issueId).Returns(issue);
        _userModuleApi.UserExistsAsync(userId).Returns(false);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await _issueService.AssignIssueToUserAsync(issueId, userId));
    }

    [Test]
    public async Task AssignIssueToTeamAsync_WithValidTeam_AssignsIssue()
    {
        // Arrange
        var issueId = IssueId.New();
        var teamId = Guid.NewGuid();
        var issue = new IssueBusinessEntity(issueId, "Bug", "Description", IssuePriority.Medium, null);
        
        _issueRepository.GetByIdAsync(issueId).Returns(issue);
        _teamModuleApi.TeamExistsAsync(teamId).Returns(true);
        _issueRepository.UpdateAsync(issue).Returns(Task.CompletedTask);

        // Act
        await _issueService.AssignIssueToTeamAsync(issueId, teamId);

        // Assert
        Assert.That(issue.AssignedTeamId, Is.EqualTo(teamId));
    }

    [Test]
    public async Task UpdateIssueStatusAsync_WithValidStatus_UpdatesStatus()
    {
        // Arrange
        var issueId = IssueId.New();
        var issue = new IssueBusinessEntity(issueId, "Bug", "Description", IssuePriority.Medium, null);
        
        _issueRepository.GetByIdAsync(issueId).Returns(issue);
        _issueRepository.UpdateAsync(issue).Returns(Task.CompletedTask);

        // Act
        await _issueService.UpdateIssueStatusAsync(issueId, IssueStatus.InProgress);

        // Assert
        Assert.That(issue.Status, Is.EqualTo(IssueStatus.InProgress));
    }

    [Test]
    public async Task DeleteIssueAsync_WithExistingIssue_DeletesIssue()
    {
        // Arrange
        var issueId = IssueId.New();
        var issue = new IssueBusinessEntity(issueId, "Bug", "Description", IssuePriority.Medium, null);
        
        _issueRepository.GetByIdAsync(issueId).Returns(issue);
        _issueRepository.DeleteAsync(issue).Returns(Task.CompletedTask);

        // Act
        await _issueService.DeleteIssueAsync(issueId);

        // Assert
        await _issueRepository.Received(1).DeleteAsync(issue);
    }
}
