using Microsoft.Extensions.Logging;
using NSubstitute;
using TicketSystem.User.Application;
using TicketSystem.User.Domain;

namespace TicketSystem.User.Tests;

[TestFixture]
public class UserServiceTests
{
    private IUserRepository _userRepository = null!;
    private ILogger<UserService> _logger = null!;
    private UserService _userService = null!;

    [SetUp]
    public void Setup()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _logger = Substitute.For<ILogger<UserService>>();
        _userService = new UserService(_userRepository, _logger);
    }

    [Test]
    public async Task CreateUserAsync_WithValidData_CreatesUser()
    {
        // Arrange
        const string email = "test@example.com";
        const string firstName = "John";
        const string lastName = "Doe";
        
        _userRepository.GetByEmailAsync(email).Returns((UserBusinessEntity?)null);
        _userRepository.AddAsync(Arg.Any<UserBusinessEntity>()).Returns(Task.CompletedTask);

        // Act
        var userId = await _userService.CreateUserAsync(email, firstName, lastName);

        // Assert
        Assert.That(userId, Is.EqualTo(0)); // ID is 0 before saving
        await _userRepository.Received(1).AddAsync(Arg.Is<UserBusinessEntity>(u => 
            u.Email == email && u.FirstName == firstName && u.LastName == lastName));
    }

    [Test]
    public async Task CreateUserAsync_WithDuplicateEmail_ThrowsException()
    {
        // Arrange
        const string email = "existing@example.com";
        var existingUser = new UserBusinessEntity(1, email, "Jane", "Doe");
        
        _userRepository.GetByEmailAsync(email).Returns(existingUser);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await _userService.CreateUserAsync(email, "John", "Doe"));
    }

    [Test]
    public async Task GetUserAsync_WithExistingUser_ReturnsUser()
    {
        // Arrange
        const long userId = 1;
        var user = new UserBusinessEntity(userId, "test@example.com", "John", "Doe");
        
        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act
        var result = await _userService.GetUserAsync(userId);

        // Assert
        Assert.That(result, Is.EqualTo(user));
    }

    [Test]
    public async Task GetUserAsync_WithNonExistingUser_ThrowsKeyNotFoundException()
    {
        // Arrange
        const long userId = 1;
        _userRepository.GetByIdAsync(userId).Returns((UserBusinessEntity?)null);

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () => 
            await _userService.GetUserAsync(userId));
    }

    [Test]
    public async Task UpdateUserAsync_WithValidData_UpdatesUser()
    {
        // Arrange
        const long userId = 1;
        var user = new UserBusinessEntity(userId, "test@example.com", "John", "Doe");
        
        _userRepository.GetByIdAsync(userId).Returns(user);
        _userRepository.UpdateAsync(user).Returns(Task.CompletedTask);

        // Act
        await _userService.UpdateUserAsync(userId, "Jane", "Smith");

        // Assert
        Assert.That(user.FirstName, Is.EqualTo("Jane"));
        Assert.That(user.LastName, Is.EqualTo("Smith"));
        await _userRepository.Received(1).UpdateAsync(user);
    }

    [Test]
    public async Task DeactivateUserAsync_WithExistingUser_DeactivatesUser()
    {
        // Arrange
        const long userId = 1;
        var user = new UserBusinessEntity(userId, "test@example.com", "John", "Doe");
        
        _userRepository.GetByIdAsync(userId).Returns(user);
        _userRepository.UpdateAsync(user).Returns(Task.CompletedTask);

        // Act
        await _userService.DeactivateUserAsync(userId);

        // Assert
        Assert.That(user.IsActive, Is.False);
        await _userRepository.Received(1).UpdateAsync(user);
    }

    [Test]
    public async Task UserExistsAsync_WithExistingUser_ReturnsTrue()
    {
        // Arrange
        const long userId = 1;
        _userRepository.ExistsAsync(userId).Returns(true);

        // Act
        var result = await _userService.UserExistsAsync(userId);

        // Assert
        Assert.That(result, Is.True);
    }
}
