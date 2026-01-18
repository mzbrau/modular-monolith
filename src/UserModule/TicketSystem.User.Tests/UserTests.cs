using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using TicketSystem.User.Application;
using TicketSystem.User.Configuration;
using TicketSystem.User.Domain;

namespace TicketSystem.User.Tests;

[TestFixture]
public class UserServiceTests
{
    private Mock<IUserRepository> _userRepository = null!;
    private Mock<ILogger<UserService>> _logger = null!;
    private Mock<IOptionsMonitor<UserSettings>> _settings = null!;
    private UserService _userService = null!;

    [SetUp]
    public void Setup()
    {
        _userRepository = new Mock<IUserRepository>();
        _logger = new Mock<ILogger<UserService>>();
        _settings = new Mock<IOptionsMonitor<UserSettings>>();
        
        // Setup default settings
        var defaultSettings = new UserSettings();
        _settings.Setup(s => s.CurrentValue).Returns(defaultSettings);
        
        _userService = new UserService(_userRepository.Object, _logger.Object, _settings.Object);
    }

    [Test]
    public async Task CreateUserAsync_WithValidData_CreatesUser()
    {
        // Arrange
        const string email = "test@example.com";
        const string firstName = "John";
        const string lastName = "Doe";
        
        _userRepository.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync((UserBusinessEntity?)null);
        _userRepository.Setup(x => x.AddAsync(It.IsAny<UserBusinessEntity>())).Returns(Task.CompletedTask);

        // Act
        var userId = await _userService.CreateUserAsync(email, firstName, lastName);

        // Assert
        Assert.That(userId, Is.EqualTo(0)); // ID is 0 before saving
        _userRepository.Verify(x => x.AddAsync(It.Is<UserBusinessEntity>(u => 
            u.Email == email && u.FirstName == firstName && u.LastName == lastName)), Times.Once);
    }

    [Test]
    public async Task CreateUserAsync_WithDuplicateEmail_ThrowsException()
    {
        // Arrange
        const string email = "existing@example.com";
        var existingUser = new UserBusinessEntity(1, email, "Jane", "Doe");
        
        _userRepository.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(existingUser);

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
        
        _userRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

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
        _userRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((UserBusinessEntity?)null);

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
        
        _userRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepository.Setup(x => x.UpdateAsync(user)).Returns(Task.CompletedTask);

        // Act
        await _userService.UpdateUserAsync(userId, "Jane", "Smith");

        // Assert
        Assert.That(user.FirstName, Is.EqualTo("Jane"));
        Assert.That(user.LastName, Is.EqualTo("Smith"));
        _userRepository.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Test]
    public async Task DeactivateUserAsync_WithExistingUser_DeactivatesUser()
    {
        // Arrange
        const long userId = 1;
        var user = new UserBusinessEntity(userId, "test@example.com", "John", "Doe");
        
        _userRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepository.Setup(x => x.UpdateAsync(user)).Returns(Task.CompletedTask);

        // Act
        await _userService.DeactivateUserAsync(userId);

        // Assert
        Assert.That(user.IsActive, Is.False);
        _userRepository.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Test]
    public async Task UserExistsAsync_WithExistingUser_ReturnsTrue()
    {
        // Arrange
        const long userId = 1;
        _userRepository.Setup(x => x.ExistsAsync(userId)).ReturnsAsync(true);

        // Act
        var result = await _userService.UserExistsAsync(userId);

        // Assert
        Assert.That(result, Is.True);
    }
}
