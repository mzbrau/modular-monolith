using Moq;
using TicketSystem.Client.Wpf.Services;
using TicketSystem.Client.Wpf.ViewModels;
using Xunit;
using IssueModel = TicketSystem.Client.Wpf.Models.Issue;
using UserModel = TicketSystem.Client.Wpf.Models.User;
using IssueStatusModel = TicketSystem.Client.Wpf.Models.IssueStatus;

namespace TicketSystem.Client.Wpf.Tests.ViewModels;

/// <summary>
/// Unit tests for confirmation dialogs in ViewModels.
/// </summary>
public class ConfirmationDialogTests
{
    /// <summary>
    /// Tests that DeleteIssueCommand shows confirmation dialog before deleting.
    /// </summary>
    [Fact]
    public async Task DeleteIssue_ShowsConfirmationDialog_BeforeDeleting()
    {
        // Arrange
        var mockIssueService = new Mock<IIssueService>();
        var mockUserService = new Mock<IUserService>();
        var mockTeamService = new Mock<ITeamService>();
        var mockDialogService = new Mock<IDialogService>();

        mockDialogService.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        var viewModel = new IssuesViewModel(
            mockIssueService.Object,
            mockUserService.Object,
            mockTeamService.Object,
            mockDialogService.Object);

        var issue = new IssueModel
        {
            Id = "test-id",
            Title = "Test Issue",
            Description = "Test Description",
            Status = IssueStatusModel.Open,
            Priority = 1,
            CreatedDate = DateTime.Now,
            LastModifiedDate = DateTime.Now
        };

        viewModel.Issues.Add(issue);
        viewModel.SelectedIssue = issue;

        // Act
        viewModel.DeleteIssueCommand.Execute(null);
        await Task.Delay(100); // Give time for async operation to complete

        // Assert
        mockDialogService.Verify(
            d => d.ShowConfirmation(
                It.Is<string>(msg => msg.Contains("Test Issue")),
                "Confirm Delete"),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeleteIssueCommand does not delete when user cancels confirmation.
    /// </summary>
    [Fact]
    public async Task DeleteIssue_DoesNotDelete_WhenUserCancelsConfirmation()
    {
        // Arrange
        var mockIssueService = new Mock<IIssueService>();
        var mockUserService = new Mock<IUserService>();
        var mockTeamService = new Mock<ITeamService>();
        var mockDialogService = new Mock<IDialogService>();

        mockDialogService.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false); // User cancels

        var viewModel = new IssuesViewModel(
            mockIssueService.Object,
            mockUserService.Object,
            mockTeamService.Object,
            mockDialogService.Object);

        var issue = new IssueModel
        {
            Id = "test-id",
            Title = "Test Issue",
            Description = "Test Description",
            Status = IssueStatusModel.Open,
            Priority = 1,
            CreatedDate = DateTime.Now,
            LastModifiedDate = DateTime.Now
        };

        viewModel.Issues.Add(issue);
        viewModel.SelectedIssue = issue;

        // Act
        viewModel.DeleteIssueCommand.Execute(null);
        await Task.Delay(100); // Give time for async operation to complete

        // Assert
        mockIssueService.Verify(
            s => s.DeleteIssueAsync(It.IsAny<string>()),
            Times.Never);
        Assert.Contains(issue, viewModel.Issues); // Issue should still be in the list
    }

    /// <summary>
    /// Tests that DeleteIssueCommand deletes when user confirms.
    /// </summary>
    [Fact]
    public async Task DeleteIssue_Deletes_WhenUserConfirms()
    {
        // Arrange
        var mockIssueService = new Mock<IIssueService>();
        var mockUserService = new Mock<IUserService>();
        var mockTeamService = new Mock<ITeamService>();
        var mockDialogService = new Mock<IDialogService>();

        mockDialogService.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true); // User confirms

        mockIssueService.Setup(s => s.DeleteIssueAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var viewModel = new IssuesViewModel(
            mockIssueService.Object,
            mockUserService.Object,
            mockTeamService.Object,
            mockDialogService.Object);

        var issue = new IssueModel
        {
            Id = "test-id",
            Title = "Test Issue",
            Description = "Test Description",
            Status = IssueStatusModel.Open,
            Priority = 1,
            CreatedDate = DateTime.Now,
            LastModifiedDate = DateTime.Now
        };

        viewModel.Issues.Add(issue);
        viewModel.SelectedIssue = issue;

        // Act
        viewModel.DeleteIssueCommand.Execute(null);
        await Task.Delay(100); // Give time for async operation to complete

        // Assert
        mockIssueService.Verify(
            s => s.DeleteIssueAsync("test-id"),
            Times.Once);
        Assert.DoesNotContain(issue, viewModel.Issues); // Issue should be removed from the list
    }

    /// <summary>
    /// Tests that DeactivateUserCommand shows confirmation dialog before deactivating.
    /// </summary>
    [Fact]
    public async Task DeactivateUser_ShowsConfirmationDialog_BeforeDeactivating()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var mockDialogService = new Mock<IDialogService>();

        mockDialogService.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        var viewModel = new UsersViewModel(mockUserService.Object, mockDialogService.Object);

        var user = new UserModel
        {
            Id = "test-id",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            DisplayName = "Test User",
            CreatedDate = DateTime.Now,
            IsActive = true
        };

        viewModel.Users.Add(user);
        viewModel.SelectedUser = user;

        // Act
        viewModel.DeactivateUserCommand.Execute(null);
        await Task.Delay(100); // Give time for async operation to complete

        // Assert
        mockDialogService.Verify(
            d => d.ShowConfirmation(
                It.Is<string>(msg => msg.Contains("test@example.com")),
                "Confirm Deactivate"),
            Times.Once);
    }

    /// <summary>
    /// Tests that DeactivateUserCommand does not deactivate when user cancels confirmation.
    /// </summary>
    [Fact]
    public async Task DeactivateUser_DoesNotDeactivate_WhenUserCancelsConfirmation()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var mockDialogService = new Mock<IDialogService>();

        mockDialogService.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false); // User cancels

        var viewModel = new UsersViewModel(mockUserService.Object, mockDialogService.Object);

        var user = new UserModel
        {
            Id = "test-id",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            DisplayName = "Test User",
            CreatedDate = DateTime.Now,
            IsActive = true
        };

        viewModel.Users.Add(user);
        viewModel.SelectedUser = user;

        // Act
        viewModel.DeactivateUserCommand.Execute(null);
        await Task.Delay(100); // Give time for async operation to complete

        // Assert
        mockUserService.Verify(
            s => s.DeactivateUserAsync(It.IsAny<string>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that DeactivateUserCommand deactivates when user confirms.
    /// </summary>
    [Fact]
    public async Task DeactivateUser_Deactivates_WhenUserConfirms()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var mockDialogService = new Mock<IDialogService>();

        mockDialogService.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true); // User confirms

        var deactivatedUser = new UserModel
        {
            Id = "test-id",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            DisplayName = "Test User",
            CreatedDate = DateTime.Now,
            IsActive = false // Deactivated
        };

        mockUserService.Setup(s => s.DeactivateUserAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        mockUserService.Setup(s => s.GetUserAsync("test-id"))
            .ReturnsAsync(deactivatedUser);

        var viewModel = new UsersViewModel(mockUserService.Object, mockDialogService.Object);

        var user = new UserModel
        {
            Id = "test-id",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            DisplayName = "Test User",
            CreatedDate = DateTime.Now,
            IsActive = true
        };

        viewModel.Users.Add(user);
        viewModel.SelectedUser = user;

        // Act
        viewModel.DeactivateUserCommand.Execute(null);
        await Task.Delay(100); // Give time for async operation to complete

        // Assert
        mockUserService.Verify(
            s => s.DeactivateUserAsync("test-id"),
            Times.Once);
    }
}
