using FluentAssertions;
using Moq;
using TicketSystem.Client.Wpf.Models;
using TicketSystem.Client.Wpf.Services;
using TicketSystem.Client.Wpf.ViewModels;
using UserModel = TicketSystem.Client.Wpf.Models.User;

namespace TicketSystem.Client.Wpf.Tests.ViewModels;

/// <summary>
/// Unit tests for UsersViewModel class.
/// Requirements: 10.1, 11.2, 12.3
/// </summary>
public class UsersViewModelTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IDialogService> _mockDialogService;

    public UsersViewModelTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockDialogService.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
    }

    [Fact]
    public void Constructor_ShouldInitializeCollectionsAndCommands()
    {
        // Act
        var viewModel = new UsersViewModel(_mockUserService.Object, _mockDialogService.Object);

        // Assert
        viewModel.Users.Should().NotBeNull();
        viewModel.Users.Should().BeEmpty();
        viewModel.LoadUsersCommand.Should().NotBeNull();
        viewModel.CreateUserCommand.Should().NotBeNull();
        viewModel.EditUserCommand.Should().NotBeNull();
        viewModel.DeactivateUserCommand.Should().NotBeNull();
        viewModel.ApplyFilterCommand.Should().NotBeNull();
    }

    [Fact]
    public async Task LoadUsersCommand_ShouldPopulateUsersCollection()
    {
        // Arrange
        var users = new List<UserModel>
        {
            new UserModel { Id = "1", Email = "user1@example.com", FirstName = "John", LastName = "Doe", DisplayName = "John Doe", IsActive = true, CreatedDate = DateTime.UtcNow },
            new UserModel { Id = "2", Email = "user2@example.com", FirstName = "Jane", LastName = "Smith", DisplayName = "Jane Smith", IsActive = true, CreatedDate = DateTime.UtcNow }
        };

        _mockUserService
            .Setup(s => s.GetAllUsersAsync())
            .ReturnsAsync(users);

        var viewModel = new UsersViewModel(_mockUserService.Object, _mockDialogService.Object);

        // Act
        viewModel.LoadUsersCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        viewModel.Users.Should().HaveCount(2);
        viewModel.Users[0].Email.Should().Be("user1@example.com");
        viewModel.Users[1].Email.Should().Be("user2@example.com");
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task LoadUsersCommand_OnError_ShouldSetErrorMessage()
    {
        // Arrange
        _mockUserService
            .Setup(s => s.GetAllUsersAsync())
            .ThrowsAsync(new Exception("Test error"));

        var viewModel = new UsersViewModel(_mockUserService.Object, _mockDialogService.Object);

        // Act
        viewModel.LoadUsersCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        viewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        viewModel.ErrorMessage.Should().Contain("error");
    }

    [Fact]
    public async Task CreateUserCommand_WithValidEmail_ShouldCallServiceAndRefreshList()
    {
        // Arrange
        _mockUserService
            .Setup(s => s.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("new-user-id");

        _mockUserService
            .Setup(s => s.GetAllUsersAsync())
            .ReturnsAsync(new List<UserModel>
            {
                new UserModel { Id = "new-user-id", Email = "newuser@example.com", FirstName = "New", LastName = "User", DisplayName = "New User", IsActive = true, CreatedDate = DateTime.UtcNow }
            });

        var viewModel = new UsersViewModel(_mockUserService.Object, _mockDialogService.Object);

        viewModel.Email = "newuser@example.com";
        viewModel.FirstName = "New";
        viewModel.LastName = "User";

        // Act
        viewModel.CreateUserCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        _mockUserService.Verify(s => s.CreateUserAsync("newuser@example.com", "New", "User"), Times.Once);
        _mockUserService.Verify(s => s.GetAllUsersAsync(), Times.Once);
        viewModel.Users.Should().HaveCount(1);
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task CreateUserCommand_WithEmptyEmail_ShouldNotCallServiceAndSetErrorMessage()
    {
        // Arrange
        var viewModel = new UsersViewModel(_mockUserService.Object, _mockDialogService.Object);

        viewModel.Email = "";
        viewModel.FirstName = "Test";
        viewModel.LastName = "User";

        // Act
        viewModel.CreateUserCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        _mockUserService.Verify(s => s.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        viewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        viewModel.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public async Task CreateUserCommand_WithInvalidEmail_ShouldNotCallServiceAndSetErrorMessage()
    {
        // Arrange
        var viewModel = new UsersViewModel(_mockUserService.Object, _mockDialogService.Object);

        viewModel.Email = "invalid-email";
        viewModel.FirstName = "Test";
        viewModel.LastName = "User";

        // Act
        viewModel.CreateUserCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        _mockUserService.Verify(s => s.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        viewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        viewModel.ErrorMessage.Should().Contain("invalid");
    }

    [Fact]
    public async Task EditUserCommand_ShouldCallServiceAndRefreshUser()
    {
        // Arrange
        var user = new UserModel
        {
            Id = "test-id",
            Email = "original@example.com",
            FirstName = "Original",
            LastName = "User",
            DisplayName = "Original User",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        _mockUserService
            .Setup(s => s.UpdateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _mockUserService
            .Setup(s => s.GetUserAsync("test-id"))
            .ReturnsAsync(new UserModel
            {
                Id = "test-id",
                Email = "original@example.com",
                FirstName = "Updated",
                LastName = "Name",
                DisplayName = "Updated Name",
                IsActive = true,
                CreatedDate = user.CreatedDate
            });

        var viewModel = new UsersViewModel(_mockUserService.Object, _mockDialogService.Object);

        viewModel.Users.Add(user);
        viewModel.SelectedUser = user;
        viewModel.FirstName = "Updated";
        viewModel.LastName = "Name";

        // Act
        viewModel.EditUserCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        _mockUserService.Verify(s => s.UpdateUserAsync("test-id", "Updated", "Name"), Times.Once);
        _mockUserService.Verify(s => s.GetUserAsync("test-id"), Times.Once);
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task DeactivateUserCommand_ShouldCallServiceAndRefreshUser()
    {
        // Arrange
        var user = new UserModel
        {
            Id = "test-id",
            Email = "user@example.com",
            FirstName = "Test",
            LastName = "User",
            DisplayName = "Test User",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        _mockUserService
            .Setup(s => s.DeactivateUserAsync("test-id"))
            .Returns(Task.CompletedTask);

        _mockUserService
            .Setup(s => s.GetUserAsync("test-id"))
            .ReturnsAsync(new UserModel
            {
                Id = "test-id",
                Email = "user@example.com",
                FirstName = "Test",
                LastName = "User",
                DisplayName = "Test User",
                IsActive = false,
                CreatedDate = user.CreatedDate
            });

        var viewModel = new UsersViewModel(_mockUserService.Object, _mockDialogService.Object);

        viewModel.Users.Add(user);
        viewModel.SelectedUser = user;

        // Act
        viewModel.DeactivateUserCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        _mockUserService.Verify(s => s.DeactivateUserAsync("test-id"), Times.Once);
        _mockUserService.Verify(s => s.GetUserAsync("test-id"), Times.Once);
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task ApplyFilterCommand_WithShowActiveOnly_ShouldFilterActiveUsers()
    {
        // Arrange
        var users = new List<UserModel>
        {
            new UserModel { Id = "1", Email = "user1@example.com", FirstName = "John", LastName = "Doe", DisplayName = "John Doe", IsActive = true, CreatedDate = DateTime.UtcNow },
            new UserModel { Id = "2", Email = "user2@example.com", FirstName = "Jane", LastName = "Smith", DisplayName = "Jane Smith", IsActive = false, CreatedDate = DateTime.UtcNow },
            new UserModel { Id = "3", Email = "user3@example.com", FirstName = "Bob", LastName = "Johnson", DisplayName = "Bob Johnson", IsActive = true, CreatedDate = DateTime.UtcNow }
        };

        _mockUserService
            .Setup(s => s.GetAllUsersAsync())
            .ReturnsAsync(users);

        var viewModel = new UsersViewModel(_mockUserService.Object, _mockDialogService.Object);

        viewModel.ShowActiveOnly = true;

        // Act
        viewModel.ApplyFilterCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        viewModel.Users.Should().HaveCount(2);
        viewModel.Users.Should().AllSatisfy(u => u.IsActive.Should().BeTrue());
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task ApplyFilterCommand_WithShowInactiveOnly_ShouldFilterInactiveUsers()
    {
        // Arrange
        var users = new List<UserModel>
        {
            new UserModel { Id = "1", Email = "user1@example.com", FirstName = "John", LastName = "Doe", DisplayName = "John Doe", IsActive = true, CreatedDate = DateTime.UtcNow },
            new UserModel { Id = "2", Email = "user2@example.com", FirstName = "Jane", LastName = "Smith", DisplayName = "Jane Smith", IsActive = false, CreatedDate = DateTime.UtcNow },
            new UserModel { Id = "3", Email = "user3@example.com", FirstName = "Bob", LastName = "Johnson", DisplayName = "Bob Johnson", IsActive = true, CreatedDate = DateTime.UtcNow }
        };

        _mockUserService
            .Setup(s => s.GetAllUsersAsync())
            .ReturnsAsync(users);

        var viewModel = new UsersViewModel(_mockUserService.Object, _mockDialogService.Object);

        viewModel.ShowInactiveOnly = true;

        // Act
        viewModel.ApplyFilterCommand.Execute(null);
        await Task.Delay(100); // Wait for async operation

        // Assert
        viewModel.Users.Should().HaveCount(1);
        viewModel.Users.Should().AllSatisfy(u => u.IsActive.Should().BeFalse());
        viewModel.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public void SelectedUser_WhenSet_ShouldUpdateFormFields()
    {
        // Arrange
        var user = new UserModel
        {
            Id = "test-id",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            DisplayName = "Test User",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        var viewModel = new UsersViewModel(_mockUserService.Object, _mockDialogService.Object);

        // Act
        viewModel.SelectedUser = user;

        // Assert
        viewModel.Email.Should().Be("test@example.com");
        viewModel.FirstName.Should().Be("Test");
        viewModel.LastName.Should().Be("User");
    }
}
