using FluentAssertions;
using Moq;
using TicketSystem.Client.Wpf.Models;
using TicketSystem.Client.Wpf.Services;
using TicketSystem.Client.Wpf.ViewModels;
using IssueModel = TicketSystem.Client.Wpf.Models.Issue;

namespace TicketSystem.Client.Wpf.Tests.ViewModels;

/// <summary>
/// Unit tests for IssuesViewModel class.
/// Requirements: 3.1, 4.2, 4.5, 6.4
/// </summary>
public class IssuesViewModelTests
{
    private readonly Mock<IIssueService> _mockIssueService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ITeamService> _mockTeamService;
    private readonly Mock<IDialogService> _mockDialogService;

    public IssuesViewModelTests()
    {
        _mockIssueService = new Mock<IIssueService>();
        _mockUserService = new Mock<IUserService>();
        _mockTeamService = new Mock<ITeamService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockDialogService.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
    }

    [Fact]
    public void Constructor_ShouldInitializeCollectionsAndCommands()
    {
        // Act
        var viewModel = new IssuesViewModel(
            _mockIssueService.Object,
            _mockUserService.Object,
            _mockTeamService.Object,
            _mockDialogService.Object);

        // Assert
        viewModel.Issues.Should().NotBeNull();
        viewModel.Issues.Should().BeEmpty();
        viewModel.LoadIssuesCommand.Should().NotBeNull();
        viewModel.CreateIssueCommand.Should().NotBeNull();
        viewModel.EditIssueCommand.Should().NotBeNull();
        viewModel.UpdateStatusCommand.Should().NotBeNull();
        viewModel.AssignToUserCommand.Should().NotBeNull();
        viewModel.AssignToTeamCommand.Should().NotBeNull();
        viewModel.DeleteIssueCommand.Should().NotBeNull();
        viewModel.ApplyFilterCommand.Should().NotBeNull();
    }

    [Fact]
    public async Task LoadIssuesCommand_ShouldPopulateIssuesCollection()
    {
        // Arrange
        var issues = new List<IssueModel>
        {
            new IssueModel { Id = "1", Title = "Issue 1", Description = "Desc 1", Status = IssueStatus.Open, Priority = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
            new IssueModel { Id = "2", Title = "Issue 2", Description = "Desc 2", Status = IssueStatus.InProgress, Priority = 2, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow }
        };

        _mockIssueService
            .Setup(s => s.GetAllIssuesAsync())
            .ReturnsAsync(issues);

        var viewModel = new IssuesViewModel(
            _mockIssueService.Object,
            _mockUserService.Object,
            _mockTeamService.Object,
            _mockDialogService.Object);

        // Act
        viewModel.LoadIssuesCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        viewModel.Issues.Should().HaveCount(2);
        viewModel.Issues[0].Title.Should().Be("Issue 1");
        viewModel.Issues[1].Title.Should().Be("Issue 2");
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task LoadIssuesCommand_OnError_ShouldSetErrorMessage()
    {
        // Arrange
        _mockIssueService
            .Setup(s => s.GetAllIssuesAsync())
            .ThrowsAsync(new Exception("Test error"));

        var viewModel = new IssuesViewModel(
            _mockIssueService.Object,
            _mockUserService.Object,
            _mockTeamService.Object,
            _mockDialogService.Object);

        // Act
        viewModel.LoadIssuesCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        viewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        viewModel.ErrorMessage.Should().Contain("error");
    }

    [Fact]
    public async Task CreateIssueCommand_WithValidTitle_ShouldCallServiceAndRefreshList()
    {
        // Arrange
        _mockIssueService
            .Setup(s => s.CreateIssueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DateTime?>()))
            .ReturnsAsync("new-issue-id");

        _mockIssueService
            .Setup(s => s.GetAllIssuesAsync())
            .ReturnsAsync(new List<IssueModel>
            {
                new IssueModel { Id = "new-issue-id", Title = "New Issue", Description = "New Desc", Status = IssueStatus.Open, Priority = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow }
            });

        var viewModel = new IssuesViewModel(
            _mockIssueService.Object,
            _mockUserService.Object,
            _mockTeamService.Object,
            _mockDialogService.Object);

        viewModel.Title = "New Issue";
        viewModel.Description = "New Desc";
        viewModel.Priority = 1;

        // Act
        viewModel.CreateIssueCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        _mockIssueService.Verify(s => s.CreateIssueAsync("New Issue", "New Desc", 1, null), Times.Once);
        _mockIssueService.Verify(s => s.GetAllIssuesAsync(), Times.Once);
        viewModel.Issues.Should().HaveCount(1);
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task CreateIssueCommand_WithEmptyTitle_ShouldNotCallServiceAndSetErrorMessage()
    {
        // Arrange
        var viewModel = new IssuesViewModel(
            _mockIssueService.Object,
            _mockUserService.Object,
            _mockTeamService.Object,
            _mockDialogService.Object);

        viewModel.Title = "";
        viewModel.Description = "Test Description";
        viewModel.Priority = 1;

        // Act
        viewModel.CreateIssueCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        _mockIssueService.Verify(s => s.CreateIssueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DateTime?>()), Times.Never);
        viewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        viewModel.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public async Task EditIssueCommand_WithValidTitle_ShouldCallServiceAndRefreshIssue()
    {
        // Arrange
        var issue = new IssueModel
        {
            Id = "test-id",
            Title = "Original Title",
            Description = "Original Description",
            Status = IssueStatus.Open,
            Priority = 1,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };

        _mockIssueService
            .Setup(s => s.UpdateIssueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DateTime?>()))
            .Returns(Task.CompletedTask);

        _mockIssueService
            .Setup(s => s.GetIssueAsync("test-id"))
            .ReturnsAsync(new IssueModel
            {
                Id = "test-id",
                Title = "Updated Title",
                Description = "Updated Description",
                Status = IssueStatus.Open,
                Priority = 2,
                CreatedDate = issue.CreatedDate,
                LastModifiedDate = DateTime.UtcNow
            });

        var viewModel = new IssuesViewModel(
            _mockIssueService.Object,
            _mockUserService.Object,
            _mockTeamService.Object,
            _mockDialogService.Object);

        viewModel.Issues.Add(issue);
        viewModel.SelectedIssue = issue;
        viewModel.Title = "Updated Title";
        viewModel.Description = "Updated Description";
        viewModel.Priority = 2;

        // Act
        viewModel.EditIssueCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        _mockIssueService.Verify(s => s.UpdateIssueAsync("test-id", "Updated Title", "Updated Description", 2, null), Times.Once);
        _mockIssueService.Verify(s => s.GetIssueAsync("test-id"), Times.Once);
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateStatusCommand_ShouldCallServiceAndRefreshIssue()
    {
        // Arrange
        var issue = new IssueModel
        {
            Id = "test-id",
            Title = "Test Issue",
            Description = "Test Description",
            Status = IssueStatus.Open,
            Priority = 1,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };

        _mockIssueService
            .Setup(s => s.UpdateIssueStatusAsync("test-id", IssueStatus.InProgress))
            .Returns(Task.CompletedTask);

        _mockIssueService
            .Setup(s => s.GetIssueAsync("test-id"))
            .ReturnsAsync(new IssueModel
            {
                Id = "test-id",
                Title = "Test Issue",
                Description = "Test Description",
                Status = IssueStatus.InProgress,
                Priority = 1,
                CreatedDate = issue.CreatedDate,
                LastModifiedDate = DateTime.UtcNow
            });

        var viewModel = new IssuesViewModel(
            _mockIssueService.Object,
            _mockUserService.Object,
            _mockTeamService.Object,
            _mockDialogService.Object);

        viewModel.Issues.Add(issue);
        viewModel.SelectedIssue = issue;
        viewModel.SelectedStatus = IssueStatus.InProgress;

        // Act
        viewModel.UpdateStatusCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        _mockIssueService.Verify(s => s.UpdateIssueStatusAsync("test-id", IssueStatus.InProgress), Times.Once);
        _mockIssueService.Verify(s => s.GetIssueAsync("test-id"), Times.Once);
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task AssignToUserCommand_ShouldCallServiceAndRefreshIssue()
    {
        // Arrange
        var issue = new IssueModel
        {
            Id = "test-id",
            Title = "Test Issue",
            Description = "Test Description",
            Status = IssueStatus.Open,
            Priority = 1,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };

        _mockIssueService
            .Setup(s => s.AssignIssueToUserAsync("test-id", "user-123"))
            .Returns(Task.CompletedTask);

        _mockIssueService
            .Setup(s => s.GetIssueAsync("test-id"))
            .ReturnsAsync(new IssueModel
            {
                Id = "test-id",
                Title = "Test Issue",
                Description = "Test Description",
                Status = IssueStatus.Open,
                Priority = 1,
                AssignedUserId = "user-123",
                CreatedDate = issue.CreatedDate,
                LastModifiedDate = DateTime.UtcNow
            });

        var viewModel = new IssuesViewModel(
            _mockIssueService.Object,
            _mockUserService.Object,
            _mockTeamService.Object,
            _mockDialogService.Object);

        viewModel.Issues.Add(issue);
        viewModel.SelectedIssue = issue;
        viewModel.SelectedUserId = "user-123";

        // Act
        viewModel.AssignToUserCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        _mockIssueService.Verify(s => s.AssignIssueToUserAsync("test-id", "user-123"), Times.Once);
        _mockIssueService.Verify(s => s.GetIssueAsync("test-id"), Times.Once);
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task AssignToTeamCommand_ShouldCallServiceAndRefreshIssue()
    {
        // Arrange
        var issue = new IssueModel
        {
            Id = "test-id",
            Title = "Test Issue",
            Description = "Test Description",
            Status = IssueStatus.Open,
            Priority = 1,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };

        _mockIssueService
            .Setup(s => s.AssignIssueToTeamAsync("test-id", "team-456"))
            .Returns(Task.CompletedTask);

        _mockIssueService
            .Setup(s => s.GetIssueAsync("test-id"))
            .ReturnsAsync(new IssueModel
            {
                Id = "test-id",
                Title = "Test Issue",
                Description = "Test Description",
                Status = IssueStatus.Open,
                Priority = 1,
                AssignedTeamId = "team-456",
                CreatedDate = issue.CreatedDate,
                LastModifiedDate = DateTime.UtcNow
            });

        var viewModel = new IssuesViewModel(
            _mockIssueService.Object,
            _mockUserService.Object,
            _mockTeamService.Object,
            _mockDialogService.Object);

        viewModel.Issues.Add(issue);
        viewModel.SelectedIssue = issue;
        viewModel.SelectedTeamId = "team-456";

        // Act
        viewModel.AssignToTeamCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        _mockIssueService.Verify(s => s.AssignIssueToTeamAsync("test-id", "team-456"), Times.Once);
        _mockIssueService.Verify(s => s.GetIssueAsync("test-id"), Times.Once);
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task DeleteIssueCommand_ShouldCallServiceAndRemoveFromCollection()
    {
        // Arrange
        var issue = new IssueModel
        {
            Id = "test-id",
            Title = "Test Issue",
            Description = "Test Description",
            Status = IssueStatus.Open,
            Priority = 1,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };

        _mockIssueService
            .Setup(s => s.DeleteIssueAsync("test-id"))
            .Returns(Task.CompletedTask);

        var viewModel = new IssuesViewModel(
            _mockIssueService.Object,
            _mockUserService.Object,
            _mockTeamService.Object,
            _mockDialogService.Object);

        viewModel.Issues.Add(issue);
        viewModel.SelectedIssue = issue;

        // Act
        viewModel.DeleteIssueCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        _mockIssueService.Verify(s => s.DeleteIssueAsync("test-id"), Times.Once);
        viewModel.Issues.Should().BeEmpty();
        viewModel.SelectedIssue.Should().BeNull();
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task DeleteIssueCommand_OnError_ShouldSetErrorMessage()
    {
        // Arrange
        var issue = new IssueModel
        {
            Id = "test-id",
            Title = "Test Issue",
            Description = "Test Description",
            Status = IssueStatus.Open,
            Priority = 1,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };

        _mockIssueService
            .Setup(s => s.DeleteIssueAsync("test-id"))
            .ThrowsAsync(new Exception("Delete failed"));

        var viewModel = new IssuesViewModel(
            _mockIssueService.Object,
            _mockUserService.Object,
            _mockTeamService.Object,
            _mockDialogService.Object);

        viewModel.Issues.Add(issue);
        viewModel.SelectedIssue = issue;

        // Act
        viewModel.DeleteIssueCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        viewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        viewModel.ErrorMessage.Should().Contain("error");
        viewModel.Issues.Should().HaveCount(1); // Issue should still be in collection
    }

    [Fact]
    public async Task ApplyFilterCommand_WithStatusFilter_ShouldFilterIssues()
    {
        // Arrange
        var issues = new List<IssueModel>
        {
            new IssueModel { Id = "1", Title = "Issue 1", Description = "Desc 1", Status = IssueStatus.Open, Priority = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
            new IssueModel { Id = "2", Title = "Issue 2", Description = "Desc 2", Status = IssueStatus.InProgress, Priority = 2, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
            new IssueModel { Id = "3", Title = "Issue 3", Description = "Desc 3", Status = IssueStatus.Open, Priority = 3, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow }
        };

        _mockIssueService
            .Setup(s => s.GetAllIssuesAsync())
            .ReturnsAsync(issues);

        var viewModel = new IssuesViewModel(
            _mockIssueService.Object,
            _mockUserService.Object,
            _mockTeamService.Object,
            _mockDialogService.Object);

        viewModel.FilterStatus = IssueStatus.Open;

        // Act
        viewModel.ApplyFilterCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        viewModel.Issues.Should().HaveCount(2);
        viewModel.Issues.Should().AllSatisfy(i => i.Status.Should().Be(IssueStatus.Open));
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public void SelectedIssue_WhenSet_ShouldUpdateFormFields()
    {
        // Arrange
        var issue = new IssueModel
        {
            Id = "test-id",
            Title = "Test Issue",
            Description = "Test Description",
            Status = IssueStatus.InProgress,
            Priority = 5,
            DueDate = DateTime.UtcNow.AddDays(7),
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };

        var viewModel = new IssuesViewModel(
            _mockIssueService.Object,
            _mockUserService.Object,
            _mockTeamService.Object,
            _mockDialogService.Object);

        // Act
        viewModel.SelectedIssue = issue;

        // Assert
        viewModel.Title.Should().Be("Test Issue");
        viewModel.Description.Should().Be("Test Description");
        viewModel.Priority.Should().Be(5);
        viewModel.SelectedStatus.Should().Be(IssueStatus.InProgress);
        viewModel.DueDate.Should().BeCloseTo(issue.DueDate.Value, TimeSpan.FromSeconds(1));
    }
}
