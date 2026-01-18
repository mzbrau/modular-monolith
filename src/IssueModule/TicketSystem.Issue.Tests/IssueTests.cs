using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using TicketSystem.Issue.Application;
using TicketSystem.Issue.Configuration;
using TicketSystem.Issue.Domain;
using TicketSystem.Team.Contracts;
using TicketSystem.User.Contracts;

namespace TicketSystem.Issue.Tests;

[TestFixture]
public class IssueServiceTests
{
    private Mock<IIssueRepository> _issueRepository = null!;
    private Mock<IUserModuleApi> _userModuleApi = null!;
    private Mock<ITeamModuleApi> _teamModuleApi = null!;
    private Mock<ILogger<IssueService>> _logger = null!;
    private Mock<IOptionsMonitor<IssueSettings>> _settings = null!;
    private IssueService _issueService = null!;

    [SetUp]
    public void Setup()
    {
        _issueRepository = new Mock<IIssueRepository>();
        _userModuleApi = new Mock<IUserModuleApi>();
        _teamModuleApi = new Mock<ITeamModuleApi>();
        _logger = new Mock<ILogger<IssueService>>();
        _settings = new Mock<IOptionsMonitor<IssueSettings>>();
        
        // Setup default settings
        var defaultSettings = new IssueSettings();
        _settings.Setup(s => s.CurrentValue).Returns(defaultSettings);
        
        _issueService = new IssueService(
            _issueRepository.Object, 
            _userModuleApi.Object, 
            _teamModuleApi.Object, 
            _logger.Object,
            _settings.Object);
    }

    [Test]
    public async Task CreateIssueAsync_WithValidData_CreatesIssue()
    {
        // Arrange
        const string title = "Bug in login";
        const string description = "Users cannot login";
        var priority = IssuePriority.High;
        DateTime? dueDate = DateTime.UtcNow.AddDays(7);
        
        _issueRepository.Setup(x => x.AddAsync(It.IsAny<IssueBusinessEntity>())).Returns(Task.CompletedTask);

        // Act
        var issueId = await _issueService.CreateIssueAsync(title, description, priority, dueDate);

        // Assert
        Assert.That(issueId, Is.GreaterThanOrEqualTo(0));
        _issueRepository.Verify(x => x.AddAsync(It.Is<IssueBusinessEntity>(t => 
            t.Title == title && t.Description == description && t.Priority == priority)), Times.Once);
    }

    [Test]
    public async Task GetIssueAsync_WithExistingIssue_ReturnsIssue()
    {
        // Arrange
        var issueId = 1L;
        var issue = new IssueBusinessEntity(issueId, "Bug", "Description", IssuePriority.Medium, null);
        
        _issueRepository.Setup(x => x.GetByIdAsync(issueId)).ReturnsAsync(issue);

        // Act
        var result = await _issueService.GetIssueAsync(issueId);

        // Assert
        Assert.That(result, Is.EqualTo(issue));
    }

    [Test]
    public async Task GetIssueAsync_WithNonExistingIssue_ThrowsKeyNotFoundException()
    {
        // Arrange
        var issueId = 1L;
        _issueRepository.Setup(x => x.GetByIdAsync(issueId)).ReturnsAsync((IssueBusinessEntity?)null);

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () => 
            await _issueService.GetIssueAsync(issueId));
    }

    [Test]
    public async Task AssignIssueToUserAsync_WithValidUser_AssignsIssue()
    {
        // Arrange
        var issueId = 1L;
        long userId = 1;
        var issue = new IssueBusinessEntity(issueId, "Bug", "Description", IssuePriority.Medium, null);
        
        _issueRepository.Setup(x => x.GetByIdAsync(issueId)).ReturnsAsync(issue);
        _userModuleApi.Setup(x => x.UserExistsAsync(It.Is<UserExistsRequest>(r => r.UserId == userId))).ReturnsAsync(true);
        _issueRepository.Setup(x => x.UpdateAsync(issue)).Returns(Task.CompletedTask);

        // Act
        await _issueService.AssignIssueToUserAsync(issueId, userId);

        // Assert
        Assert.That(issue.AssignedUserId, Is.EqualTo(userId));
        _issueRepository.Verify(x => x.UpdateAsync(issue), Times.Once);
    }

    [Test]
    public async Task AssignIssueToUserAsync_WithNonExistingUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var issueId = 1L;
        long userId = 1;
        var issue = new IssueBusinessEntity(issueId, "Bug", "Description", IssuePriority.Medium, null);
        
        _issueRepository.Setup(x => x.GetByIdAsync(issueId)).ReturnsAsync(issue);
        _userModuleApi.Setup(x => x.UserExistsAsync(It.Is<UserExistsRequest>(r => r.UserId == userId))).ReturnsAsync(false);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await _issueService.AssignIssueToUserAsync(issueId, userId));
    }

    [Test]
    public async Task AssignIssueToTeamAsync_WithValidTeam_AssignsIssue()
    {
        // Arrange
        var issueId = 1L;
        long teamId = 1;
        var issue = new IssueBusinessEntity(issueId, "Bug", "Description", IssuePriority.Medium, null);
        
        _issueRepository.Setup(x => x.GetByIdAsync(issueId)).ReturnsAsync(issue);
        _teamModuleApi.Setup(x => x.TeamExistsAsync(It.Is<TeamExistsRequest>(r => r.TeamId == teamId))).ReturnsAsync(true);
        _issueRepository.Setup(x => x.UpdateAsync(issue)).Returns(Task.CompletedTask);

        // Act
        await _issueService.AssignIssueToTeamAsync(issueId, teamId);

        // Assert
        Assert.That(issue.AssignedTeamId, Is.EqualTo(teamId));
    }

    [Test]
    public async Task UpdateIssueStatusAsync_WithValidStatus_UpdatesStatus()
    {
        // Arrange
        var issueId = 1L;
        var issue = new IssueBusinessEntity(issueId, "Bug", "Description", IssuePriority.Medium, null);
        
        _issueRepository.Setup(x => x.GetByIdAsync(issueId)).ReturnsAsync(issue);
        _issueRepository.Setup(x => x.UpdateAsync(issue)).Returns(Task.CompletedTask);

        // Act
        await _issueService.UpdateIssueStatusAsync(issueId, IssueStatus.InProgress);

        // Assert
        Assert.That(issue.Status, Is.EqualTo(IssueStatus.InProgress));
    }

    [Test]
    public async Task DeleteIssueAsync_WithExistingIssue_DeletesIssue()
    {
        // Arrange
        var issueId = 1L;
        var issue = new IssueBusinessEntity(issueId, "Bug", "Description", IssuePriority.Medium, null);
        
        _issueRepository.Setup(x => x.GetByIdAsync(issueId)).ReturnsAsync(issue);
        _issueRepository.Setup(x => x.DeleteAsync(issue)).Returns(Task.CompletedTask);

        // Act
        await _issueService.DeleteIssueAsync(issueId);

        // Assert
        _issueRepository.Verify(x => x.DeleteAsync(issue), Times.Once);
    }
}
